using NUnit.Framework;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SystemMonitor.Core.Models;
using SystemMonitor.Infrastructure.Plugins;

namespace SystemMonitor.Tests.Plugins
{
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        public string? LastContent { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            LastContent = await request.Content!.ReadAsStringAsync();
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    [TestFixture]
    public class ApiPostPluginTests
    {
        [Test]
        public async Task OnMetricsAsync_ShouldPostCorrectJson()
        {
            // Arrange
            var handler = new FakeHttpMessageHandler();
            var client = new HttpClient(handler);
            var plugin = new ApiPostPlugin(client, "http://localhost/metrics");

            var data = new MonitoringData
            {
                Timestamp = DateTime.UtcNow,
                CpuPercent = 42,
                RamUsedMb = 2048,
                RamTotalMb = 4096,
                DiskUsedMb = 10000,
                DiskTotalMb = 20000
            };


            // Act
            await plugin.OnMetricsAsync(data);

            // Assert
            Assert.That(handler.LastContent, Does.Contain("\"cpu\":42"));
            Assert.That(handler.LastContent, Does.Contain("\"ram_used\":2048"));
            Assert.That(handler.LastContent, Does.Contain("\"disk_used\":10000"));
        }
    }
}
