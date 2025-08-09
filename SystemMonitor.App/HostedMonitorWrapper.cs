namespace SystemMonitor.App;

using Microsoft.Extensions.Hosting;
using SystemMonitor.Core.Services;

/// <summary>
/// Small adapter that connects IMonitoringService to the Generic Host lifetime via IHostedService.
/// Keeps MonitoringService testable while allowing simple host integration.
/// </summary>
public class HostedMonitorWrapper : IHostedService
{
    private readonly IMonitoringService _monitoringService;

    public HostedMonitorWrapper(IMonitoringService monitoringService)
    {
        _monitoringService = monitoringService;
    }

    public Task StartAsync(CancellationToken cancellationToken) => _monitoringService.StartAsync(cancellationToken);

    public Task StopAsync(CancellationToken cancellationToken) => _monitoringService.StopAsync(cancellationToken);
}