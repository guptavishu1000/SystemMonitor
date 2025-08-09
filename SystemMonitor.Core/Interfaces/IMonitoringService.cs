namespace SystemMonitor.Core.Services;

/// <summary>
/// Abstraction over the monitoring runner. Allows testing and customization.
/// </summary>
public interface IMonitoringService
{
    /// <summary>
    /// Start monitoring (non-blocking). The host typically calls StartAsync on hosted services.
    /// </summary>
    Task StartAsync(CancellationToken ct = default);

    /// <summary>
    /// Stop monitoring.
    /// </summary>
    Task StopAsync(CancellationToken ct = default);
}