namespace SystemMonitor.Core.Interfaces;

using SystemMonitor.Core.Models;

/// <summary>
/// Strategy interface for platform-specific metrics collection.
/// Implementations should be lightweight and thread-safe.
/// </summary>
public interface ISystemMetricsProvider
{
    /// <summary>
    /// Get a single metrics sample. Implementations may block briefly to sample counters if required.
    /// </summary>
    MonitoringData GetMetrics();
}
