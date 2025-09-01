using System;
using DotNetApi.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetApi.Tests
{
    public class GreetingRepositoryFactoryTests
    {
        [Fact]
        public void Create_ThrowsOnNullProvider()
        {
            var config = new ConfigurationBuilder().Build();
            var logger = new Mock<ILogger<GreetingSqlRepository>>().Object;
            var cache = new Mock<IDistributedCache>().Object;
            Assert.Throws<ArgumentNullException>(() => GreetingRepositoryFactory.CreateForProviderOrThrow(config, null, logger, cache));
        }

        [Fact]
        public void Create_ThrowsOnUnsupportedProvider()
        {
            var config = new ConfigurationBuilder().Build();
            var logger = new Mock<ILogger<GreetingSqlRepository>>().Object;
            var cache = new Mock<IDistributedCache>().Object;
            Assert.Throws<NotSupportedException>(() => GreetingRepositoryFactory.CreateForProviderOrThrow(config, "Unknown", logger, cache));
        }
    }
}
