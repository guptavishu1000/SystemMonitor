namespace SystemMonitor.Infrastructure.Platform;

using SystemMonitor.Core.Interfaces;

/// <summary>
/// Factory that selects an ISystemMetricsProvider implementation using the Strategy pattern.
/// The selection can be based on RuntimeInformation, configuration, environment, or explicit override.
/// New providers can be registered here or via DI registration extension methods.
/// </summary>
public static class ProviderFactory
{
    public static ISystemMetricsProvider CreateProvider(string? preferredProvider = null)
    {
        // NOTE: This is intentionally simple. For real apps, use DI to register multiple providers
        // and select them by name (IOptions pattern) or implement a registry to resolve by key.

        if (!string.IsNullOrEmpty(preferredProvider))
        {
            // Choose by configured name ("windows", "linux", ...)
            return preferredProvider.ToLowerInvariant() switch
            {
                "windows" => new Providers.WindowsMetricsProvider(),
                "linux" => new Providers.LinuxMetricsProvider(),
                _ => throw new ArgumentException("Unknown provider name", nameof(preferredProvider))
            };
        }

        // Fallback: pick based on runtime OS
        if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            return new Providers.WindowsMetricsProvider();

        return new Providers.LinuxMetricsProvider();
    }
}