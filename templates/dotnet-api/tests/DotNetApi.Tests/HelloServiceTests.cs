using Microsoft.Extensions.Logging;
using DotNetApi.Application.Services;
using DotNetApi.Domain.Entities;
using DotNetApi.Domain.Interfaces;
using Moq;

namespace DotNetApi.Tests
{
        public class HelloServiceTests
        {
            private readonly Mock<IGreetingRepository> _repoMock;
            private readonly Mock<ILogger<HelloService>> _loggerMock;
            private readonly HelloService _service;

            public HelloServiceTests()
            {
                _repoMock = new Mock<IGreetingRepository>();
                _loggerMock = new Mock<ILogger<HelloService>>();
                _service = new HelloService(_repoMock.Object, _loggerMock.Object);
            }

            [Fact]
            public async Task GetLastGreetingAsync_Throws_WhenRepositoryThrows()
            {
                _repoMock.Setup(r => r.GetLastGreetingAsync()).ThrowsAsync(new InvalidOperationException("DB error"));
                await Assert.ThrowsAsync<InvalidOperationException>(async () => await _service.GetLastGreetingAsync());
            }

            [Fact]
            public async Task GetGreetingByIdAsync_LogsError_WhenRepositoryThrows()
            {
                _repoMock.Setup(r => r.GetGreetingByIdAsync(1)).ThrowsAsync(new Exception("fail"));
                await Assert.ThrowsAsync<Exception>(async () => await _service.GetGreetingByIdAsync(1));
                _loggerMock.Verify(l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.AtLeastOnce);
            }

            [Fact]
            public async Task SaveGreetingAsync_LogsError_WhenRepositoryThrows()
            {
                var greeting = new Greeting { Id = 1, Message = "Olá", CreatedAt = DateTime.UtcNow };
                _repoMock.Setup(r => r.SaveGreetingAsync(greeting)).ThrowsAsync(new Exception("fail"));
                await Assert.ThrowsAsync<Exception>(async () => await _service.SaveGreetingAsync(greeting));
                _loggerMock.Verify(l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), Times.AtLeastOnce);
            }

            [Fact]
            public async Task GetPagedGreetingsAsync_Throws_WhenRepositoryThrows()
            {
                _repoMock.Setup(r => r.GetGreetingsAsync(1, 10, "CreatedAt", true, null)).ThrowsAsync(new Exception("fail"));
                await Assert.ThrowsAsync<Exception>(async () => await _service.GetPagedGreetingsAsync(1, 10, "CreatedAt", true, null));
            }

            [Fact]
            public async Task GetPagedGreetingsAsync_ReturnsPagedDtoWithLinks()
            {
                var greetings = new List<Greeting>
                {
                    new Greeting { Id = 1, Message = "Oi", CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
                    new Greeting { Id = 2, Message = "Olá", CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
                    new Greeting { Id = 3, Message = "Hello", CreatedAt = DateTime.UtcNow }
                };
                _repoMock.Setup(r => r.GetGreetingsAsync(1, 2, "CreatedAt", true, null)).ReturnsAsync(greetings);
                var result = await _service.GetPagedGreetingsAsync(1, 2, "CreatedAt", true, null);
                Assert.Equal(3, result.TotalItems);
                Assert.Equal(2, result.PageSize);
                Assert.Equal(1, result.Page);
                Assert.Equal(3, result.Items.Count);
                Assert.Contains(result.Links, l => l.Rel == "self");
                Assert.Contains(result.Links, l => l.Rel == "create");
                Assert.Contains(result.Links, l => l.Rel == "next");
            }

            [Fact]
            public async Task GetPagedGreetingsAsync_ReturnsEmpty_WhenNoResults()
            {
                _repoMock.Setup(r => r.GetGreetingsAsync(1, 10, "CreatedAt", true, null)).ReturnsAsync(new List<Greeting>());
                var result = await _service.GetPagedGreetingsAsync(1, 10, "CreatedAt", true, null);
                Assert.Empty(result.Items);
                Assert.Equal(0, result.TotalItems);
                Assert.Contains(result.Links, l => l.Rel == "self");
            }

            [Fact]
            public async Task GetPagedGreetingsAsync_AppliesFilterAndOrdering()
            {
                var greetings = new List<Greeting>
                {
                    new Greeting { Id = 1, Message = "Oi", CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
                    new Greeting { Id = 2, Message = "Olá", CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
                    new Greeting { Id = 3, Message = "Hello", CreatedAt = DateTime.UtcNow }
                };
                _repoMock.Setup(r => r.GetGreetingsAsync(1, 10, "Message", false, "Olá")).ReturnsAsync(greetings.Where(g => g.Message.Contains("Olá")));
                var result = await _service.GetPagedGreetingsAsync(1, 10, "Message", false, "Olá");
                Assert.Single(result.Items);
                Assert.Equal("Olá", result.Items[0].Message);
            }

            [Fact]
            public async Task GetPagedGreetingsAsync_LinksAreCorrectForFirstAndLastPage()
            {
                var greetings = new List<Greeting>
                {
                    new Greeting { Id = 1, Message = "Oi", CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
                    new Greeting { Id = 2, Message = "Olá", CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
                    new Greeting { Id = 3, Message = "Hello", CreatedAt = DateTime.UtcNow }
                };
                // Página 1
                _repoMock.Setup(r => r.GetGreetingsAsync(1, 2, "CreatedAt", true, null)).ReturnsAsync(greetings);
                var result1 = await _service.GetPagedGreetingsAsync(1, 2, "CreatedAt", true, null);
                Assert.Contains(result1.Links, l => l.Rel == "next");
                Assert.DoesNotContain(result1.Links, l => l.Rel == "prev");
                // Página 2
                _repoMock.Setup(r => r.GetGreetingsAsync(2, 2, "CreatedAt", true, null)).ReturnsAsync(greetings);
                var result2 = await _service.GetPagedGreetingsAsync(2, 2, "CreatedAt", true, null);
                Assert.Contains(result2.Links, l => l.Rel == "prev");
            }

            [Fact]
            public async Task GetAllGreetingsAsync_Throws_WhenRepositoryThrows()
            {
                _repoMock.Setup(r => r.GetAllGreetingsAsync()).ThrowsAsync(new Exception("fail"));
                await Assert.ThrowsAsync<Exception>(async () => await _service.GetAllGreetingsAsync());
            }

            [Fact]
            public async Task SaveGreetingAsync_ConcurrentCalls_AreHandled()
            {
                var greeting1 = new Greeting { Id = 1, Message = "Olá", CreatedAt = DateTime.UtcNow };
                var greeting2 = new Greeting { Id = 2, Message = "Hello", CreatedAt = DateTime.UtcNow };
                _repoMock.Setup(r => r.SaveGreetingAsync(It.IsAny<Greeting>())).Returns(Task.CompletedTask);
                var t1 = _service.SaveGreetingAsync(greeting1);
                var t2 = _service.SaveGreetingAsync(greeting2);
                await Task.WhenAll(t1, t2);
                _repoMock.Verify(r => r.SaveGreetingAsync(greeting1), Times.Once);
                _repoMock.Verify(r => r.SaveGreetingAsync(greeting2), Times.Once);
            }

            [Fact]
            public async Task SaveGreetingAsync_ValidatesAllRules()
            {
                // Id inválido
                var greeting = new Greeting { Id = 0, Message = "Olá", CreatedAt = DateTime.UtcNow };
                await Assert.ThrowsAsync<ArgumentException>(async () => await _service.SaveGreetingAsync(greeting));

                // Mensagem nula
                greeting = new Greeting { Id = 1, Message = null!, CreatedAt = DateTime.UtcNow };
                await Assert.ThrowsAsync<ArgumentException>(async () => await _service.SaveGreetingAsync(greeting));

                // Mensagem vazia
                greeting = new Greeting { Id = 1, Message = "", CreatedAt = DateTime.UtcNow };
                await Assert.ThrowsAsync<ArgumentException>(async () => await _service.SaveGreetingAsync(greeting));

                // Mensagem longa
                greeting = new Greeting { Id = 1, Message = new string('a', 201), CreatedAt = DateTime.UtcNow };
                await Assert.ThrowsAsync<ArgumentException>(async () => await _service.SaveGreetingAsync(greeting));
            }

            [Fact]
            public async Task GetAllGreetingsAsync_ReturnsAll()
            {
                var greetings = new List<Greeting>
                {
                    new Greeting { Id = 1, Message = "Oi", CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
                    new Greeting { Id = 2, Message = "Olá", CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
                    new Greeting { Id = 3, Message = "Hello", CreatedAt = DateTime.UtcNow }
                };
                _repoMock.Setup(r => r.GetAllGreetingsAsync()).ReturnsAsync(greetings);
                var result = await _service.GetAllGreetingsAsync();
                Assert.Equal(3, result.Count());
            }

            [Fact]
            public async Task GetAllGreetingsAsync_ReturnsEmpty_WhenNoGreetings()
            {
                _repoMock.Setup(r => r.GetAllGreetingsAsync()).ReturnsAsync(new List<Greeting>());
                var result = await _service.GetAllGreetingsAsync();
                Assert.Empty(result);
            }
        }
    }
