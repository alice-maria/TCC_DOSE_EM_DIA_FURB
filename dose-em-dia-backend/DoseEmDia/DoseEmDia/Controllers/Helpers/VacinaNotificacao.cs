using DoseEmDia.Models;
using DoseEmDia.Models.db;
using DoseEmDia.Models.Enums;
using Microsoft.EntityFrameworkCore;
using DoseEmDia.Helpers;

namespace DoseEmDia.Controllers.Helpers
{
    public class VacinaNotificacao : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public VacinaNotificacao(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
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

                    var vacinas = await context.Vacina
                        .Include(v => v.Usuario)
                        .Where(v => v.ValidadeMeses.HasValue)
                        .ToListAsync(stoppingToken);

                    foreach (var vacina in vacinas)
                    {
                        var dataVencimento = vacina.DataAplicacao.AddMonths(vacina.ValidadeMeses ?? 12);

                        TipoNotificacao? tipo = null;

                        if (dataVencimento.Date == dataAlvo)
                        {
                            tipo = TipoNotificacao.VacinaVencendo;
                        }
                        else if (dataVencimento.Date < hoje)
                        {
                            tipo = TipoNotificacao.VacinaAtrasada;
                        }

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
                                await emailService.EnviarEmailAsync(vacina.Usuario.Email, titulo, mensagem);

                                context.Notificacao.Add(new Notificacao
                                {
                                    UsuarioId = vacina.UsuarioId,
                                    Titulo = titulo,
                                    Mensagem = mensagem,
                                    Tipo = tipo.Value,
                                    DataEnvio = DateTime.Now,
                                    Visualizada = false
                                });

                                await context.SaveChangesAsync(stoppingToken);
                            }
                        }
                    }
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}
