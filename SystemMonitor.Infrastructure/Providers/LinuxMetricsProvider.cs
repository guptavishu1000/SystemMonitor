namespace SystemMonitor.Infrastructure.Providers;

using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

/// <summary>
/// Linux implementation - stubbed for architecture.
/// </summary>
public class LinuxMetricsProvider : ISystemMetricsProvider
{
    public LinuxMetricsProvider()
    {
        // initialize any readers (e.g., /proc parsing utilities)
    }

    public MonitoringData GetMetrics()
    {
        // TODO: implement using /proc/stat, /proc/meminfo, and DriveInfo.
        throw new NotImplementedException();
    }
}