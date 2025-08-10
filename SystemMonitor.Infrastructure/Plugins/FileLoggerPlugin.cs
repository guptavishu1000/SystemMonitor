using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;

namespace SystemMonitor.Infrastructure.Plugins;

public class FileLoggerPlugin : IMonitorPlugin
{
    private readonly string _filePath;
    private readonly SemaphoreSlim _sema = new(1, 1);

    public FileLoggerPlugin(string filePath)
    {
        _filePath = filePath ?? "monitor.log";
    }

    public async Task OnMetricsAsync(MonitoringData data, CancellationToken ct = default)
    {
        var line = $"{data.Timestamp:O} | CPU:{data.CpuPercent:F1}% | RAM:{data.RamUsedMb:F0}/{data.RamTotalMb:F0} MB | DISK:{data.DiskUsedMb:F0}/{data.DiskTotalMb:F0} MB";
        try
        {
            await _sema.WaitAsync(ct).ConfigureAwait(false);
            await File.AppendAllTextAsync(_filePath, line + Environment.NewLine, ct).ConfigureAwait(false);
        }
        catch (OperationCanceledException) { }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"FileLoggerPlugin error: {ex.Message}");
        }
        finally { _sema.Release(); }
    }
}