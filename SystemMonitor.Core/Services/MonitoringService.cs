using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;
using SystemMonitor.Core.Services;

namespace SystemMonitor.App.Services;

public class MonitoringService : IMonitoringService, IDisposable
{
    private readonly ISystemMetricsProvider _provider;
    private readonly IEnumerable<IMonitorPlugin> _plugins;
    private readonly ILogger<MonitoringService> _logger;
    private Timer? _timer;
    private readonly TimeSpan _interval;
    private int _running = 0;

    public MonitoringService(ISystemMetricsProvider provider, IEnumerable<IMonitorPlugin> plugins, ILogger<MonitoringService> logger, IConfiguration config)
    {
        _provider = provider;
        _plugins = plugins;
        _logger = logger;
        _interval = TimeSpan.FromSeconds(
            int.TryParse(config["MonitoringInterval"], out var s) ? s : 5
        );
    }

    public Task StartAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting monitoring with interval {Interval}s", _interval.TotalSeconds);
        _timer = new Timer(Tick, null, TimeSpan.Zero, _interval);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Stopping monitoring");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async void Tick(object? state)
    {
        if (Interlocked.Exchange(ref _running, 1) == 1) return;
        try
        {
            MonitoringData sample;
            try { sample = _provider.GetMetrics(); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to collect metrics");
                return;
            }

            Console.WriteLine($"{sample.Timestamp:O} | CPU:{sample.CpuPercent:F1}% | RAM:{sample.RamUsedMb:F0}/{sample.RamTotalMb:F0} MB | DISK:{sample.DiskUsedMb:F0}/{sample.DiskTotalMb:F0} MB");

            var tasks = _plugins.Select(p => SafeRunPluginAsync(p, sample));
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        finally { Interlocked.Exchange(ref _running, 0); }
    }

    private async Task SafeRunPluginAsync(IMonitorPlugin plugin, MonitoringData data)
    {
        try { await plugin.OnMetricsAsync(data).ConfigureAwait(false); }
        catch (Exception ex) { _logger.LogError(ex, "Plugin {Plugin} failed", plugin.GetType().FullName); }
    }

    public void Dispose() => _timer?.Dispose();
}