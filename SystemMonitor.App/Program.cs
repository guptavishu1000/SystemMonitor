using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SystemMonitor.App;
using SystemMonitor.Infrastructure.Plugins;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((ctx, cfg) => cfg.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true))
    .ConfigureServices((ctx, services) =>
    {
        services.AddLogging();
        services.AddSystemMonitorServices(ctx.Configuration);

        // Register ApiPostPlugin with endpoint from configuration
        //services.AddHttpClient<ApiPostPlugin>((sp, client) =>
        //{
        //    var cfg = sp.GetRequiredService<IConfiguration>();
        //    var endpoint = cfg["Api:Endpoint"];
        //    if (string.IsNullOrWhiteSpace(endpoint))
        //        throw new ArgumentException("API endpoint must be provided", nameof(endpoint));

        //    new ApiPostPlugin(client, endpoint);
        //});

        //services.AddHttpClient("ApiPlugin");
        //services.AddSingleton<IMonitorPlugin>(sp =>
        //    new ApiPostPlugin(
        //        sp.GetRequiredService<IHttpClientFactory>().CreateClient("ApiPlugin"),
        //        ctx.Configuration["Api:Endpoint"] ?? ""));

        // Bridge IMonitoringService to hosted lifetime
        services.AddHostedService<HostedWrapper>();
    })

    .UseConsoleLifetime();

await builder.RunConsoleAsync();