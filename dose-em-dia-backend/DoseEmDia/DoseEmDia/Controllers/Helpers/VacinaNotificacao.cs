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
                        .Where(v => v.ValidadeMeses.HasValue &&
                                    v.Usuario.ReceberNotificacoes &&
                                    (v.DataAplicacao.AddMonths(v.ValidadeMeses.Value) <= dataAlvo ||
                                     v.DataAplicacao.AddMonths(v.ValidadeMeses.Value) < hoje))
                        .ToListAsync(stoppingToken);

                    var notificacoesCriadas = new List<Notificacao>();

                    foreach (var vacina in vacinas)
                    {
                        var dataVencimento = vacina.DataAplicacao.AddMonths(vacina.ValidadeMeses.Value);
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

                            // Verifica se já foi enviada
                            bool jaEnviado = await context.Notificacao.AnyAsync(n =>
                                n.UsuarioId == vacina.UsuarioId &&
                                n.Tipo == tipo &&
                                n.Mensagem.Contains(vacina.Nome),
                                stoppingToken);

                            if (!jaEnviado)
                            {
                                // Envia o e-mail
                                await emailService.EnviarEmailAsync(vacina.Usuario.Email, titulo, mensagem);

                                // Adiciona a notificação
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

                    // Salva todas as notificações de uma vez
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