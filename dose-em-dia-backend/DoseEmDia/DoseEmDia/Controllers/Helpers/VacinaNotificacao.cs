using DoseEmDia.Models;
using DoseEmDia.Models.db;
using DoseEmDia.Models.Enums;
using Microsoft.EntityFrameworkCore;
using DoseEmDia.Helpers;
using Microsoft.Extensions.Logging;

namespace DoseEmDia.Controllers.Helpers
{
    public class VacinaNotificacao : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<VacinaNotificacao> _logger;

        public VacinaNotificacao(IServiceProvider serviceProvider, ILogger<VacinaNotificacao> logger)
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

                                string mensagem = $"Atenção! A vacina {vacina.Nome} está {titulo.ToLower()} e precisa de sua atenção.";

                                bool jaEnviado = await context.Notificacao.AnyAsync(n =>
                                    n.UsuarioId == vacina.UsuarioId &&
                                    n.Tipo == tipo &&
                                    n.Mensagem.Contains(vacina.Nome),
                                    stoppingToken);

                                if (!jaEnviado)
                                {
                                    try
                                    {
                                        await emailService.EnviarEmailAsync(vacina.Usuario.Email, titulo, mensagem);
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.LogError(ex, $"Falha ao enviar e-mail para {vacina.Usuario.Email} - Método: ExecuteAsync");
                                        continue;
                                    }

                                    notificacoesCriadas.Add(new Notificacao
                                    {
                                        UsuarioId = vacina.UsuarioId,
                                        Titulo = titulo,
                                        Mensagem = mensagem,
                                        Tipo = tipo.Value,
                                        DataEnvio = DateTime.Now,
                                        Visualizada = false
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Erro ao processar vacina ID {vacina.Id} - Método: ExecuteAsync");
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