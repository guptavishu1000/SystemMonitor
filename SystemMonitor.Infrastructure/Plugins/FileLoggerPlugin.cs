namespace SystemMonitor.Infrastructure.Plugins;

using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

/// <summary>
/// Simple plugin that logs human-readable lines to a rolling file.
/// </summary>
public class FileLoggerPlugin : IMonitorPlugin
{
    private readonly string _path;

    public FileLoggerPlugin(string path)
    {
        _path = path;
    }

    public Task OnMetricsAsync(MonitoringData data, CancellationToken ct = default)
    {
        // Minimal example: append to file. In production, use a proper async logger and rotation.
        var line = $"{data.Timestamp:o} | CPU:{data.CpuPercent:F1}% | RAM:{data.RamUsedMb:F0}/{data.RamTotalMb:F0} MB | DISK:{data.DiskUsedMb:F0}/{data.DiskTotalMb:F0} MB";
        File.AppendAllText(_path, line + Environment.NewLine);
        return Task.CompletedTask;
    }
}
