using DoseEmDia.Helpers;

namespace DoseEmDia.Services
{
    public class CampanhasEmailService : BackgroundService
    {
        private readonly ILogger<CampanhasEmailService> _logger;
        private readonly IServiceProvider _services;
        private FileSystemWatcher _watcher;

        public CampanhasEmailService(ILogger<CampanhasEmailService> logger, IServiceProvider services)
        {
            _logger = logger;
            _services = services;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var pasta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagens", "campanhas");
            if (!Directory.Exists(pasta)) Directory.CreateDirectory(pasta);

            _watcher = new FileSystemWatcher(pasta)
            {
                Filter = "*.*",
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite
            };

            _watcher.Created += async (s, e) =>
            {
                try
                {
                    _logger.LogInformation("Nova imagem detectada: {Path}", e.FullPath);

                    await Task.Delay(2000, stoppingToken); 

                    using var scope = _services.CreateScope();
                    var envio = scope.ServiceProvider.GetRequiredService<EnvioEmail>();

                    await envio.DispararCampanhaAsync(e.FullPath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao disparar campanha automática.");
                }
            };

            _watcher.EnableRaisingEvents = true;
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _watcher?.Dispose();
            base.Dispose();
        }
    }
}
