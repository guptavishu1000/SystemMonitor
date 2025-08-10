using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SystemMonitor.App.Services;
using SystemMonitor.Core.Interfaces;
using SystemMonitor.Core.Services;
using SystemMonitor.Infrastructure.Platform;
using SystemMonitor.Infrastructure.Plugins;

namespace SystemMonitor.App;

public static class ServiceRegistration
{
    public static IServiceCollection AddSystemMonitorServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<ISystemMetricsProvider>(sp => ProviderFactory.CreateProvider(config["Provider:Name"]));

        // Register plugins statically
        var logPath = config["Logging:FilePath"] ?? "monitor.log";
        services.AddSingleton<IMonitorPlugin>(sp => new FileLoggerPlugin(logPath));
        services.AddHttpClient("ApiPlugin");
        services.AddSingleton<IMonitorPlugin>(sp => new ApiPostPlugin(sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiPlugin"), config["Api:Endpoint"] ?? string.Empty));

        services.AddSingleton<IMonitoringService, MonitoringService>();

        return services;
    }
}