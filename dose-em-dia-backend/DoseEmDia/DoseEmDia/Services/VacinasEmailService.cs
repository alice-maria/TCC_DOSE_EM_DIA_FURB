using DoseEmDia.Helpers;
using DoseEmDia.Models;
using DoseEmDia.Models.db;
using DoseEmDia.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace DoseEmDia.Services
{
    public class VacinasEmailService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VacinasEmailService> _logger;
        private const int WINDOW_DAYS = 30;
        private const int DEDUPE_DAYS = 31;
        private const int MAX_POR_USUARIO = 2;
        private static readonly TimeSpan CADENCIA = TimeSpan.FromMinutes(5);

        public VacinasEmailService(IServiceProvider serviceProvider, ILogger<VacinasEmailService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("VacinasEmailService iniciado em {ts}", DateTimeOffset.Now);

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var emailService = scope.ServiceProvider.GetRequiredService<EnvioEmail>();

                var hoje = DateTime.Today;
                var dataAlvo = hoje.AddDays(WINDOW_DAYS);

                List<Vacina> vacinas = new();

                try
                {
                    vacinas = await context.Vacina
                        .AsNoTracking()
                        .Include(v => v.Usuario)
                        .Where(v => v.ValidadeMeses.HasValue
                                 && v.DataAplicacao != null
                                 && v.Usuario != null
                                 && v.Usuario.ReceberNotificacoes)
                        .ToListAsync(stoppingToken);

                    _logger.LogInformation(
                        "Consulta base retornou {qtde} vacinas (aplicadas, com validade e usuários aptos a notificação).",
                        vacinas.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao buscar vacinas.");
                }

                var candidatos = vacinas
                    .Select(v =>
                    {
                        var venc = v.DataAplicacao!.AddMonths(v.ValidadeMeses ?? 12).Date;
                        TipoNotificacao? tipo = null;
                        if (venc < hoje) tipo = TipoNotificacao.VacinaAtrasada;
                        else if (venc <= dataAlvo) tipo = TipoNotificacao.VacinaVencendo;
                        return new { v, venc, tipo };
                    })
                    .Where(x => x.tipo != null)
                    .OrderBy(x => x.tipo == TipoNotificacao.VacinaAtrasada ? 0 : 1)
                    .ThenBy(x => x.venc)
                    .ToList();

                _logger.LogInformation("Candidatos a notificação: {qtde}", candidatos.Count);

                if (candidatos.Count == 0)
                {
                    try
                    {
                        var semEmail = vacinas.Count(x => string.IsNullOrWhiteSpace(x.Usuario!.Email));
                        var totalUsuariosSemNotif = await context.Usuario.CountAsync(u => !u.ReceberNotificacoes, stoppingToken);
                        _logger.LogInformation(
                            "Diagnóstico: usuários sem e-mail (na base atual)={semEmail}; usuários com ReceberNotificacoes=false={qtdeSemNotif}.",
                            semEmail, totalUsuariosSemNotif);
                    }
                    catch { /* diagnóstico best-effort */ }
                }

                var notificacoesCriadas = new List<Notificacao>();
                var emailsPorUsuario = new Dictionary<int, int>();

                foreach (var c in candidatos)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var vacina = c.v;
                    var dataVencimento = c.venc;
                    var tipo = c.tipo!.Value;

                    if (vacina.Usuario == null) continue;
                    if (string.IsNullOrWhiteSpace(vacina.Usuario.Email))
                    {
                        _logger.LogWarning("Usuário {UsuarioId} sem e-mail. VacinaId={VacinaId}",
                            vacina.UsuarioId, vacina.IdVacina);
                        continue;
                    }

                    if (emailsPorUsuario.TryGetValue(vacina.UsuarioId, out int enviados) &&
                        enviados >= MAX_POR_USUARIO)
                        continue;

                    string titulo = tipo == TipoNotificacao.VacinaAtrasada
                        ? $"Vacina atrasada: {vacina.Nome}"
                        : $"Vacina prestes a vencer: {vacina.Nome}";

                    string h2Titulo = tipo == TipoNotificacao.VacinaAtrasada
                        ? "Vacina em atraso"
                        : "Vacina prestes a vencer";

                    string statusTexto = tipo == TipoNotificacao.VacinaAtrasada
                        ? "em atraso"
                        : "prestes a vencer";

                    string mensagem = $@"
<html>
  <body style='font-family: Arial, Helvetica, sans-serif; background-color: #f9f9f9; padding: 20px; color: #333;'>
    <div style='max-width: 600px; margin: auto; background: #ffffff; border-radius: 8px; padding: 25px; box-shadow: 0 2px 6px rgba(0,0,0,0.1);'>
      <h2 style='color: #d93025; text-align: center;'>{h2Titulo}</h2>
      <p>Olá,</p>
      <p>
        Identificamos que a vacina <strong>{vacina.Nome}</strong> está <strong>{statusTexto}</strong>.
        <br/>Data de vencimento estimada: <strong>{dataVencimento:dd/MM/yyyy}</strong>.
      </p>
      <p>
        Manter sua vacinação em dia é essencial para garantir sua saúde e proteção.
        Caso já tenha regularizado, por favor, desconsidere este aviso.
      </p>
      <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;' />
      <p style='font-size: 13px; color: #666; text-align: center;'>
        Este é um aviso automático do sistema <strong>Dose em Dia</strong>.<br/>
        Não responda a este e-mail diretamente.
      </p>
    </div>
  </body>
</html>";

                    bool jaEnviado = false;
                    try
                    {
                        var cutoff = DateTime.Now.AddDays(-DEDUPE_DAYS);
                        jaEnviado = await context.Notificacao.AnyAsync(n =>
                            n.UsuarioId == vacina.UsuarioId
                            && n.Tipo == tipo
                            && n.Titulo == titulo
                            && n.Mensagem.Contains(vacina.Nome)
                            && n.DataEnvio > cutoff, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Falha ao verificar deduplicação para UsuarioId={UsuarioId}, VacinaId={VacinaId}",
                            vacina.UsuarioId, vacina.IdVacina);
                        jaEnviado = false; // fallback: deixa passar
                    }

                    if (jaEnviado)
                    {
                        _logger.LogInformation("Notificação já enviada recentemente para UsuarioId={UsuarioId}, Vacina={Vacina}.",
                            vacina.UsuarioId, vacina.Nome);
                        continue;
                    }

                    bool emailEnviado = false;
                    try
                    {
                        _logger.LogInformation("Enviando e-mail para {Email} | UsuarioId={UsuarioId} | Vacina={Vacina} | Tipo={Tipo} | Venc={Venc}",
                            vacina.Usuario.Email, vacina.UsuarioId, vacina.Nome, tipo, dataVencimento.ToString("yyyy-MM-dd"));

                        await emailService.EnviarEmailAsync(vacina.Usuario.Email, titulo, mensagem);
                        emailEnviado = true;

                        emailsPorUsuario[vacina.UsuarioId] =
                            emailsPorUsuario.TryGetValue(vacina.UsuarioId, out var ctt) ? ctt + 1 : 1;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Falha ao enviar e-mail para {Email} (VacinaId={VacinaId})",
                            vacina.Usuario.Email, vacina.IdVacina);
                    }

                    notificacoesCriadas.Add(new Notificacao
                    {
                        UsuarioId = vacina.UsuarioId,
                        Titulo = titulo,
                        Mensagem = mensagem,
                        Tipo = tipo,
                        DataEnvio = DateTime.Now,
                        Visualizada = false,
                        EmailEnviado = emailEnviado
                    });
                }

                if (notificacoesCriadas.Count > 0)
                {
                    try
                    {
                        context.Notificacao.AddRange(notificacoesCriadas);
                        await context.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Notificações registradas: {qtde} (ok={ok}, falhas={falhas})",
                            notificacoesCriadas.Count,
                            notificacoesCriadas.Count(n => n.EmailEnviado),
                            notificacoesCriadas.Count(n => !n.EmailEnviado));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao salvar notificações.");
                    }
                }
                else
                {
                    _logger.LogInformation("Nenhuma notificação criada nesta execução.");
                }

                try
                {
                    await Task.Delay(CADENCIA, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("VacinasEmailService finalizado em {ts}", DateTimeOffset.Now);
        }
    }
}