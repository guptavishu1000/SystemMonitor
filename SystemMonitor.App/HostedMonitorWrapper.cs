using Microsoft.Extensions.Hosting;
using SystemMonitor.Core.Services;

public class HostedWrapper : IHostedService
{
    private readonly IMonitoringService _svc;
    public HostedWrapper(IMonitoringService svc) => _svc = svc;
    public Task StartAsync(CancellationToken cancellationToken) => _svc.StartAsync(cancellationToken);
    public Task StopAsync(CancellationToken cancellationToken) => _svc.StopAsync(cancellationToken);
}