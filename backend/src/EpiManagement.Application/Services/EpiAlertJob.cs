using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EpiManagement.Application.Services;

public class EpiAlertJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EpiAlertJob> _logger;

    public EpiAlertJob(IServiceScopeFactory scopeFactory, ILogger<EpiAlertJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var alertService = scope.ServiceProvider.GetRequiredService<AlertService>();
                var config = await GetConfigAsync(scope, stoppingToken);

                var alertHour = config?.AlertHour ?? 6;
                var now = DateTime.Now;
                var nextRun = now.Date.AddHours(alertHour);
                if (nextRun <= now) nextRun = nextRun.AddDays(1);

                var delay = nextRun - now;
                _logger.LogInformation("EpiAlertJob: próximo envio em {NextRun:dd/MM/yyyy HH:mm}", nextRun);
                await Task.Delay(delay, stoppingToken);

                if (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("EpiAlertJob: enviando alertas de EPIs vencidos...");
                    await alertService.SendOverdueAlertsAsync(stoppingToken);
                    _logger.LogInformation("EpiAlertJob: concluído.");
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                _logger.LogError(ex, "EpiAlertJob: erro ao enviar alertas");
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }
    }

    private static async Task<EpiManagement.Domain.Entities.SystemConfig?> GetConfigAsync(IServiceScope scope, CancellationToken ct)
    {
        var uow = scope.ServiceProvider.GetRequiredService<EpiManagement.Domain.Interfaces.IUnitOfWork>();
        return await uow.SystemConfig.GetAsync(ct);
    }
}
