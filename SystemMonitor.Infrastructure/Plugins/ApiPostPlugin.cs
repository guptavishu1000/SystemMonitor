namespace SystemMonitor.Infrastructure.Plugins;

using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Models;
using System.Net.Http;
using System.Text.Json;

/// <summary>
/// Plugin that posts JSON payloads to a configured API endpoint.
/// </summary>
public class ApiPostPlugin : IMonitorPlugin
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;

    public ApiPostPlugin(HttpClient httpClient, string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("API endpoint must be provided", nameof(endpoint));

        _httpClient = httpClient;
        _endpoint = endpoint;
    }

    public async Task OnMetricsAsync(MonitoringData data, CancellationToken ct = default)
    {
        var payload = new
        {
            cpu = data.CpuPercent,
            ram_used = data.RamUsedMb,
            disk_used = data.DiskUsedMb
        };

        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Fire-and-forget style but awaiting to allow retry logic insertion.
        try
        {
            var resp = await _httpClient.PostAsync(_endpoint, content, ct).ConfigureAwait(false);
            resp.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            // Plugins should catch/log their own errors; don't let exceptions propagate to core loop.
            Console.Error.WriteLine($"ApiPostPlugin error: {ex.Message}");
        }
    }
}
