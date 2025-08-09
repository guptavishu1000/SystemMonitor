namespace SystemMonitor.Infrastructure.Providers;

using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

/// <summary>
/// Windows implementation - stubbed for architecture.
/// Implement actual sampling logic later. Must implement GetMetrics.
/// </summary>
public class WindowsMetricsProvider : ISystemMetricsProvider
{
    public WindowsMetricsProvider()
    {
        // initialize counters, P/Invoke setup, etc.
    }

    public MonitoringData GetMetrics()
    {
        // TODO: implement using PerformanceCounter or Win32 APIs behind abstractions.
        throw new NotImplementedException();
    }
}