using DoseEmDia.Helpers;
using DoseEmDia.Models.db;
using DoseEmDia.Models.Enums;
using Microsoft.EntityFrameworkCore;

public sealed class VacinasEmailService : BackgroundService
{
    private readonly IServiceProvider _sp;
    private readonly ILogger<VacinasEmailService> _logger;

    private const int RUN_HOUR_LOCAL = 9;   
    private const int RUN_MINUTE_LOCAL = 0;
    private static readonly TimeZoneInfo TZ = GetBrazilTz();

    public VacinasEmailService(IServiceProvider sp, ILogger<VacinasEmailService> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("VacinasEmailService iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var delay = TempoAteProximaExecucao();
                _logger.LogInformation("Próxima execução em {delay} (à(s) {hora} hora local).",
                    delay, $"{RUN_HOUR_LOCAL:D2}:{RUN_MINUTE_LOCAL:D2}");

                await Task.Delay(delay, stoppingToken);

                await RunOnceAsync(stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // encerrando
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no loop do VacinasEmailService.");
                try { await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); } catch { /* ignore */ }
            }
        }

        _logger.LogInformation("VacinasEmailService finalizado.");
    }

    private async Task RunOnceAsync(CancellationToken ct)
    {
        using var scope = _sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var mailer = scope.ServiceProvider.GetRequiredService<EnvioEmail>();

        var agoraUtc = DateTime.UtcNow;
        var hojeLocal = TimeZoneInfo.ConvertTimeFromUtc(agoraUtc, TZ).Date;

        _logger.LogInformation("Iniciando varredura diária {dataLocal}.", hojeLocal.ToString("yyyy-MM-dd"));

        var usuarios = await db.Usuario
            .AsNoTracking()
            .Where(u => u.ReceberNotificacoes == true && !string.IsNullOrEmpty(u.Email))
            .Select(u => new { u.IdUser, u.Email })
            .ToListAsync(ct);

        foreach (var u in usuarios)
        {
            try
            {
                bool jaEnviouHoje = await db.Notificacao
                    .AsNoTracking()
                    .AnyAsync(n =>
                        n.UsuarioId == u.IdUser
                        && (n.Tipo == TipoNotificacao.VacinaAtrasada || n.Tipo == TipoNotificacao.VacinaVencendo)
                        && TimeZoneInfo.ConvertTimeFromUtc(n.DataEnvio, TZ).Date == hojeLocal, ct);

                if (jaEnviouHoje)
                    continue;

                bool temStatusRelevante = await db.Vacina
                    .AsNoTracking()
                    .AnyAsync(v =>
                        v.UsuarioId == u.IdUser &&
                        (v.Status == StatusVacina.EmAtraso || v.Status == StatusVacina.AVencer), ct);

                if (!temStatusRelevante)
                    continue;

                await mailer.EnviarResumoVacinasPorStatusAsync(u.IdUser, ct);
            }
            catch (Exception exUser)
            {
                _logger.LogWarning(exUser, "Falha ao processar usuário {IdUser} no envio diário.", u.IdUser);
            }
        }

        _logger.LogInformation("Varredura diária concluída.");
    }

    private static TimeSpan TempoAteProximaExecucao()
    {
        var agoraUtc = DateTime.UtcNow;
        var agoraLocal = TimeZoneInfo.ConvertTimeFromUtc(agoraUtc, TZ);

        var proximaExecucaoLocal = new DateTime(
            agoraLocal.Year, agoraLocal.Month, agoraLocal.Day,
            RUN_HOUR_LOCAL, RUN_MINUTE_LOCAL, 0, agoraLocal.Kind);

        if (proximaExecucaoLocal <= agoraLocal)
            proximaExecucaoLocal = proximaExecucaoLocal.AddDays(1);

        var proximaExecucaoUtc = TimeZoneInfo.ConvertTimeToUtc(proximaExecucaoLocal, TZ);
        var delay = proximaExecucaoUtc - agoraUtc;
        return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
    }

    private static TimeZoneInfo GetBrazilTz()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo"); }
        catch
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time"); }
            catch { return TimeZoneInfo.Local; }
        }
    }
}
