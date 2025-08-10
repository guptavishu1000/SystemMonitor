using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SystemMonitor.App;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) => cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true))
    .ConfigureServices((ctx, services) =>
    {
        services.AddLogging();
        services.AddSystemMonitorServices(ctx.Configuration);
        services.AddHostedService<HostedWrapper>();
    })
    .UseConsoleLifetime();

await builder.RunConsoleAsync();