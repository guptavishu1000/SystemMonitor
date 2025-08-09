namespace SystemMonitor.Core.Models;

/// <summary>
/// A single sample of system metrics.
/// </summary>
public record MonitoringData
{
    /// <summary>CPU usage percentage (0-100)</summary>
    public double CpuPercent { get; init; }

    /// <summary>RAM used in MB</summary>
    public double RamUsedMb { get; init; }

    /// <summary>RAM total in MB</summary>
    public double RamTotalMb { get; init; }

    /// <summary>Disk used in MB</summary>
    public double DiskUsedMb { get; init; }

    /// <summary>Disk total in MB</summary>
    public double DiskTotalMb { get; init; }

    /// <summary>Timestamp of the sample</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}