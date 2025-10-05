using DoseEmDia.Models;
using DoseEmDia.Models.db;
using DoseEmDia.Models.Enums;
using Microsoft.EntityFrameworkCore;
using DoseEmDia.Helpers;

namespace DoseEmDia.Services
{
    public class VacinasEmailService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VacinasEmailService> _logger;

        public VacinasEmailService(IServiceProvider serviceProvider, ILogger<VacinasEmailService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var emailService = scope.ServiceProvider.GetRequiredService<EnvioEmail>();

                    var hoje = DateTime.Today;
                    var dataAlvo = hoje.AddDays(30);

                    List<Vacina> vacinas = new();

                    try
                    {
                        vacinas = await context.Vacina
                            .Include(v => v.Usuario)
                            .Where(v => v.ValidadeMeses.HasValue
                                     && v.DataAplicacao != null
                                     && v.Usuario != null
                                     && v.Usuario.ReceberNotificacoes)
                            .ToListAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao buscar vacinas no método ExecuteAsync.");
                    }

                    var candidatos = vacinas
                        .Select(v =>
                        {
                            var venc = v.DataAplicacao.AddMonths(v.ValidadeMeses ?? 12).Date;
                            TipoNotificacao? tipo = null;
                            if (venc < hoje) tipo = TipoNotificacao.VacinaAtrasada;
                            else if (venc >= hoje && venc <= dataAlvo) tipo = TipoNotificacao.VacinaVencendo;
                            return new { v, venc, tipo };
                        })
                        .Where(x => x.tipo != null)
                        .OrderBy(x => x.tipo == TipoNotificacao.VacinaAtrasada ? 0 : 1) 
                        .ThenBy(x => x.venc) 
                        .ToList();

                    var notificacoesCriadas = new List<Notificacao>();

                    Dictionary<int, int> emailsPorUsuario = new();
                    const int MAX_POR_USUARIO = 2;

                    foreach (var c in candidatos)
                    {
                        var vacina = c.v;
                        var dataVencimento = c.venc;
                        var tipo = c.tipo!.Value;

                        if (emailsPorUsuario.TryGetValue(vacina.UsuarioId, out int enviados) &&
                            enviados >= MAX_POR_USUARIO)
                        {
                            continue; 
                        }

                        try
                        {
                            string titulo = tipo == TipoNotificacao.VacinaAtrasada
                                ? "Vacina atrasada"
                                : "Vacina prestes a vencer";

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
                                    Identificamos que a vacina <strong>{vacina.Nome}</strong> está <strong>{statusTexto}</strong> e requer a sua atenção imediata.
                                  </p>
                                  <p>
                                    Manter sua vacinação em dia é essencial para garantir sua saúde e proteção contra doenças preveníveis. Caso já tenha regularizado sua vacinação, por favor, desconsidere este aviso.
                                  </p>
                                  
                                  <hr style='border: none; border-top: 1px solid #eee; margin: 30px 0;' />
                                  <p style='font-size: 13px; color: #666; text-align: center;'>
                                    Este é um aviso automático do sistema <strong>Dose em Dia</strong>.<br/>
                                    Não responda a este e-mail diretamente.
                                  </p>
                                </div>
                              </body>
                            </html>";

                            bool jaEnviado = await context.Notificacao.AnyAsync(n =>
                                n.UsuarioId == vacina.UsuarioId
                                && n.Tipo == tipo
                                && n.Titulo == titulo
                                && n.Mensagem.Contains(vacina.Nome),
                                stoppingToken);

                            if (!jaEnviado)
                            {
                                bool emailEnviado = false;

                                try
                                {
                                    await emailService.EnviarEmailAsync(vacina.Usuario.Email, titulo, mensagem);
                                    emailEnviado = true;

                                    if (!emailsPorUsuario.ContainsKey(vacina.UsuarioId))
                                        emailsPorUsuario[vacina.UsuarioId] = 0;

                                    emailsPorUsuario[vacina.UsuarioId]++;
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
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao processar vacina ID {VacinaId}", vacina.IdVacina);
                        }
                    }

                    if (notificacoesCriadas.Any())
                    {
                        try
                        {
                            context.Notificacao.AddRange(notificacoesCriadas);
                            await context.SaveChangesAsync(stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Erro ao salvar notificações.");
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}