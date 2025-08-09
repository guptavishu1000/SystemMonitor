using SystemMonitor.Core.Models;

/// <summary>
/// Plugin interface. Each plugin receives metrics samples and may perform I/O or remote calls.
/// Plugins should handle their own errors and not throw from OnMetricsAsync.
/// </summary>
public interface IMonitorPlugin
{
    /// <summary>
    /// Called whenever a new MonitoringData sample is available.
    /// Implementations should avoid long blocking synchronous work; async-friendly patterns are encouraged.
    /// </summary>
    Task OnMetricsAsync(MonitoringData data, CancellationToken ct = default);
}