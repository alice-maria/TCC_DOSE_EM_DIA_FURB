using DoseEmDia.Models;
using DoseEmDia.Models.db;
using DoseEmDia.Models.Enums;
using Microsoft.EntityFrameworkCore;
using DoseEmDia.Helpers;

namespace DoseEmDia.Controllers.Helpers
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
                            .Where(v => v.ValidadeMeses.HasValue &&
                                        v.Usuario.ReceberNotificacoes &&
                                        (v.DataAplicacao.AddMonths(v.ValidadeMeses.Value) <= dataAlvo ||
                                         v.DataAplicacao.AddMonths(v.ValidadeMeses.Value) < hoje))
                            .ToListAsync(stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao buscar vacinas no método ExecuteAsync.");
                    }

                    var notificacoesCriadas = new List<Notificacao>();

                    foreach (var vacina in vacinas)
                    {
                        try
                        {
                            var dataVencimento = vacina.DataAplicacao.AddMonths(vacina.ValidadeMeses ?? 12);

                            TipoNotificacao? tipo = null;

                            if (dataVencimento.Date == dataAlvo)
                                tipo = TipoNotificacao.VacinaVencendo;
                            else if (dataVencimento.Date < hoje)
                                tipo = TipoNotificacao.VacinaAtrasada;

                            if (tipo != null)
                            {
                                string titulo = tipo == TipoNotificacao.VacinaAtrasada
                                    ? "Vacina atrasada"
                                    : "Vacina prestes a vencer";

                                string mensagem = $@"
                                <html>
                                  <body style='font-family: Arial, Helvetica, sans-serif; background-color: #f9f9f9; padding: 20px; color: #333;'>
                                    <div style='max-width: 600px; margin: auto; background: #ffffff; border-radius: 8px; padding: 25px; box-shadow: 0 2px 6px rgba(0,0,0,0.1);'>
                                      
                                      <h2 style='color: #d93025; text-align: center;'>⚠️ Vacina em atraso</h2>
                                      
                                      <p>Olá,</p>
                                      <p>
                                        Identificamos que a vacina <strong>{vacina.Nome}</strong> está <strong>em atraso</strong> e requer a sua atenção imediata.
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
                                    n.UsuarioId == vacina.UsuarioId &&
                                    n.Tipo == tipo &&
                                    n.Mensagem.Contains(vacina.Nome),
                                    stoppingToken);

                                if (!jaEnviado)
                                {
                                    bool emailEnviado = false;

                                    try
                                    {
                                        await emailService.EnviarEmailAsync(vacina.Usuario.Email, titulo, mensagem);
                                        emailEnviado = true;
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, $"Falha ao enviar e-mail para {vacina.Usuario.Email} - Método: ExecuteAsync");
                                    }

                                    notificacoesCriadas.Add(new Notificacao
                                    {
                                        UsuarioId = vacina.UsuarioId,
                                        Titulo = titulo,
                                        Mensagem = mensagem,
                                        Tipo = tipo.Value,
                                        DataEnvio = DateTime.Now,
                                        Visualizada = false,
                                        EmailEnviado = emailEnviado
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Erro ao processar vacina ID {vacina.IdVacina} - Método: ExecuteAsync");
                        }
                    }

                    if (notificacoesCriadas.Any())
                    {
                        context.Notificacao.AddRange(notificacoesCriadas);
                        await context.SaveChangesAsync(stoppingToken);
                    }
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}