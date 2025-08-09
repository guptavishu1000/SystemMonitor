using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using SystemMonitor.App;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((hostingContext, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        services.AddLogging();
        services.AddSystemMonitorServices(context.Configuration);

        // Optionally register the hosted background service wrapper that calls IMonitoringService
        services.AddHostedService<HostedMonitorWrapper>();
    })
    .UseConsoleLifetime();

var host = builder.Build();
await host.RunAsync();