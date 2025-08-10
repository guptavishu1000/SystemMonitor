using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using SystemMonitor.Core.Models;
using SystemMonitor.Infrastructure.Plugins;

namespace SystemMonitor.Tests.Plugins
{
    [TestFixture]
    public class FileLoggerPluginTests
    {
        [Test]
        public async Task OnMetricsAsync_ShouldWriteExpectedLine_ToFile()
        {
            // Arrange
            var tempFile = Path.GetTempFileName();
            var plugin = new FileLoggerPlugin(tempFile);
            var data = new MonitoringData
            {
                Timestamp = DateTime.UtcNow,
                CpuPercent = 50.5,
                RamUsedMb = 4096,
                RamTotalMb = 8192,
                DiskUsedMb = 20000,
                DiskTotalMb = 50000
            };

            // Act
            await plugin.OnMetricsAsync(data);

            // Assert
            var text = File.ReadAllText(tempFile);
            Assert.That(text, Does.Contain("CPU:50.5%"));
            Assert.That(text, Does.Contain("RAM:4096/8192 MB"));
            Assert.That(text, Does.Contain("DISK:20000/50000 MB"));
        }
    }
}
