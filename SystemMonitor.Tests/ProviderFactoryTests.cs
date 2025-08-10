using NUnit.Framework;
using SystemMonitor.Infrastructure.Providers;
using Microsoft.Extensions.Configuration;
using System.Runtime.InteropServices;
using SystemMonitor.Infrastructure.Platform;

namespace SystemMonitor.Tests.Platform
{
    [TestFixture]
    public class ProviderFactoryTests
    {
        [TestCase("windows", typeof(WindowsMetricsProvider))]
        //[TestCase("linux", typeof(LinuxMetricsProvider))]
        //[TestCase("macos", typeof(MacMetricsProvider))]
        public void CreateProvider_ShouldReturnExpectedType_WhenConfigured(string providerName, Type expectedType)
        {
            // Arrange
            var inMemorySettings = new Dictionary<string, string?>
            {
                {"Provider:Name", providerName}
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings!)
                .Build();

            // Act
            var provider = ProviderFactory.CreateProvider(config["Provider:Name"]);

            // Assert
            Assert.That(provider, Is.InstanceOf(expectedType));
        }
    }
}
