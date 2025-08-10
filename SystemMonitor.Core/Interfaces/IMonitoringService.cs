namespace SystemMonitor.Core.Services;

public interface IMonitoringService
{
    Task StartAsync(CancellationToken ct = default);
    Task StopAsync(CancellationToken ct = default);
}