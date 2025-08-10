namespace SystemMonitor.Core.Models;

/// <summary>
/// A single sample of system metrics.
/// </summary>
public record MonitoringData
{
    public double CpuPercent { get; init; }
    public double RamUsedMb { get; init; }
    public double RamTotalMb { get; init; }
    public double DiskUsedMb { get; init; }
    public double DiskTotalMb { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}