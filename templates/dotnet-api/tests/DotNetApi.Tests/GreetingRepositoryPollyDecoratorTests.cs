using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotNetApi.Domain.Entities;
using DotNetApi.Domain.Interfaces;
using DotNetApi.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace DotNetApi.Tests
{
    public class GreetingRepositoryPollyDecoratorTests
    {
        [Fact]
        public async Task GetLastGreetingAsync_RetriesOnException()
        {
            var innerMock = new Mock<IGreetingRepository>();
            int callCount = 0;
            innerMock.Setup(r => r.GetLastGreetingAsync())
                .Callback(() => callCount++)
                .ThrowsAsync(new Exception("fail"));
            var decorator = new GreetingRepositoryPollyDecorator(innerMock.Object);
            await Assert.ThrowsAnyAsync<Exception>(async () => await decorator.GetLastGreetingAsync());
            Assert.True(callCount > 1); // Polly deve tentar mais de uma vez
        }

        [Fact]
        public async Task SaveGreetingAsync_RetriesOnException()
        {
            var innerMock = new Mock<IGreetingRepository>();
            int callCount = 0;
            innerMock.Setup(r => r.SaveGreetingAsync(It.IsAny<Greeting>()))
                .Callback(() => callCount++)
                .ThrowsAsync(new Exception("fail"));
            var decorator = new GreetingRepositoryPollyDecorator(innerMock.Object);
            await Assert.ThrowsAnyAsync<Exception>(async () => await decorator.SaveGreetingAsync(new Greeting { Id = 1, Message = "msg", CreatedAt = DateTime.UtcNow }));
            Assert.True(callCount > 1);
        }

        [Fact]
        public async Task GetGreetingByIdAsync_RetriesOnException()
        {
            var innerMock = new Mock<IGreetingRepository>();
            int callCount = 0;
            innerMock.Setup(r => r.GetGreetingByIdAsync(It.IsAny<int>()))
                .Callback(() => callCount++)
                .ThrowsAsync(new Exception("fail"));
            var decorator = new GreetingRepositoryPollyDecorator(innerMock.Object);
            await Assert.ThrowsAnyAsync<Exception>(async () => await decorator.GetGreetingByIdAsync(1));
            Assert.True(callCount > 1);
        }

        [Fact]
        public async Task GetGreetingsAsync_RetriesOnException()
        {
            var innerMock = new Mock<IGreetingRepository>();
            int callCount = 0;
            innerMock.Setup(r => r.GetGreetingsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<string>()))
                .Callback(() => callCount++)
                .ThrowsAsync(new Exception("fail"));
            var decorator = new GreetingRepositoryPollyDecorator(innerMock.Object);
            await Assert.ThrowsAnyAsync<Exception>(async () => await decorator.GetGreetingsAsync(1, 10, "CreatedAt", true, null));
            Assert.True(callCount > 1);
        }

        [Fact]
        public async Task GetAllGreetingsAsync_RetriesOnException()
        {
            var innerMock = new Mock<IGreetingRepository>();
            int callCount = 0;
            innerMock.Setup(r => r.GetAllGreetingsAsync())
                .Callback(() => callCount++)
                .ThrowsAsync(new Exception("fail"));
            var decorator = new GreetingRepositoryPollyDecorator(innerMock.Object);
            await Assert.ThrowsAnyAsync<Exception>(async () => await decorator.GetAllGreetingsAsync());
            Assert.True(callCount > 1);
        }

        [Fact]
        public async Task GetLastGreetingAsync_ReturnsValue()
        {
            var innerMock = new Mock<IGreetingRepository>();
            var greeting = new Greeting { Id = 1, Message = "msg", CreatedAt = DateTime.UtcNow };
            innerMock.Setup(r => r.GetLastGreetingAsync()).ReturnsAsync(greeting);
            var decorator = new GreetingRepositoryPollyDecorator(innerMock.Object);
            var result = await decorator.GetLastGreetingAsync();
            Assert.NotNull(result);
            Assert.Equal(greeting.Id, result.Id);
        }
    }
}
