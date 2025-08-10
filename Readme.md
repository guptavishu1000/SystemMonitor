# SystemMonitor

SystemMonitor is a cross-platform .NET console application that samples system metrics (CPU, RAM, Disk) on a configurable interval and dispatches metrics to plugins (file logger, HTTP API poster, etc.).

## Features
- Periodic sampling of CPU%, RAM used/total, Disk used/total
- Extensible plugin system (`IMonitorPlugin`) for extra behavior
- Platform-specific metrics providers abstracted by `ISystemMetricsProvider`
- Dependency injection, Generic Host, and clean architecture (Used Plugin Design Pattern, etc)
- Unit tests with NUnit

## Project structure
- `SystemMonitor.Core` — models and interfaces
- `SystemMonitor.Infrastructure` — provider and plugin implementations
- `SystemMonitor.App` — host, DI, configuration, and monitoring loop
- `SystemMonitor.Tests` — NUnit unit tests

## Requirements
- .NET 6 SDK or later (Here I used .NET 8))
- (optional) Visual Studio or VS Code

## Quickstart
1. Restore & build:
```bash
dotnet restore
dotnet build
```
2. Run the application:
```bash
dotnet run --project SystemMonitor.App
```
3. Configure plugins in `appsettings.json`:
```json
{
  "MonitoringIntervalSeconds": 5,
  "Provider": { "Name": "" },
  "Api": { "Endpoint": "http://localhost:5000/metrics" },
  "Logging": { "FilePath": "monitor.log" }
}
```
4. Run Tests:
```bash
dotnet test SystemMonitor.Tests
```

## Run a Local Test Receiver (Avoid 403 Forbidden)

To prevent `403 Forbidden` responses while developing and testing the `ApiPostPlugin`, run a local test receiver that accepts POST requests from SystemMonitor. This local endpoint simulates the real API. This is accomplished by creating a simple ASP.NET Core Web API project that listens for POST requests on `/metrics`.

1. Create a new ASP.NET Core Web API project:
```bash
dotnet new webapi -n DummyApi
cd DummyApi
```
2. Make the API accept metrics:
Edit Program.cs in the DummyApi project:
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/metrics", async (HttpContext ctx) =>
{
    using var reader = new StreamReader(ctx.Request.Body);
    var body = await reader.ReadToEndAsync();
    Console.WriteLine("Received POST /metrics:");
    Console.WriteLine(body);

    // Return success
    return Results.Ok(new { status = "received", time = DateTime.UtcNow });
});

app.Run("http://localhost:5000");
```
3. Run the DummyApi project:
```bash
dotnet run
```
You’ll see something like:
```
Now listening on: http://localhost:5000
```
