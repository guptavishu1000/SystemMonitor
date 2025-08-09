namespace SystemMonitor.App;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Services;
using SystemMonitor.Infrastructure.Platform;
using SystemMonitor.Infrastructure.Plugins;

public static class ServiceRegistration
{
    public static IServiceCollection AddSystemMonitorServices(this IServiceCollection services, IConfiguration config)
    {
        // Configure and register providers via factory or direct DI registration.
        // For architecture clarity, we register a factory that resolves the concrete provider.

        services.AddSingleton<ISystemMetricsProvider>(sp =>
        {
            var preferred = config["Provider:Name"]; // optional
            return ProviderFactory.CreateProvider(preferred);
        });

        // Register plugins (static registration). Later this can be dynamic (load from folder).
        services.AddSingleton<IMonitorPlugin>(sp => new FileLoggerPlugin(config["Logging:FilePath"] ?? "monitor.log"));
        services.AddHttpClient("ApiPlugin");
        services.AddSingleton<IMonitorPlugin>(sp => new ApiPostPlugin(sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiPlugin"), config["Api:Endpoint"] ?? string.Empty));

        // Register monitoring service implementation (BackgroundService or custom)
        services.AddSingleton<IMonitoringService, Services.MonitoringService>();

        return services;
    }
}
