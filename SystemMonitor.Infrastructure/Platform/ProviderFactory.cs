using SystemMonitor.Core.Interfaces;
using SystemMonitor.Infrastructure.Providers;

namespace SystemMonitor.Infrastructure.Platform;

public static class ProviderFactory
{
    public static ISystemMetricsProvider CreateProvider(string? name = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            return name.ToLowerInvariant() switch
            {
                "windows" => new WindowsMetricsProvider(),
                "linux" => new LinuxMetricsProvider(),
                "macos" or "darwin" or "osx" => new MacMetricsProvider(),
                _ => throw new ArgumentException("Unknown provider name")
            };
        }

        if (OperatingSystem.IsWindows()) return new WindowsMetricsProvider();
        if (OperatingSystem.IsLinux()) return new LinuxMetricsProvider();
        if (OperatingSystem.IsMacOS()) return new MacMetricsProvider();

        throw new PlatformNotSupportedException("Unsupported OS");
    }
}