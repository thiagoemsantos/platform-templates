using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DotNetApi.Domain.Entities;
using DotNetApi.Infrastructure.Repositories;
using MongoDB.Driver;
using Moq;
using Xunit;

namespace DotNetApi.Tests
{
    public class GreetingMongoRepositoryTests
    {
        [Fact]
        public void Constructor_ThrowsOnNullConnectionString()
        {
            Assert.Throws<ArgumentNullException>(() => new GreetingMongoRepository(null!, "db"));
        }

        [Fact]
        public void Constructor_ThrowsOnNullDbName()
        {
            Assert.Throws<ArgumentNullException>(() => new GreetingMongoRepository("mongodb://localhost", null!));
        }


        [Fact]
        public async Task GetLastGreetingAsync_MockCollection_ReturnsGreeting()
        {
            var mockCollection = new Mock<IMongoCollection<Greeting>>();
            var mockCursor = new Mock<IAsyncCursor<Greeting>>();
            var greeting = new Greeting { Id = 1, Message = "Hello", CreatedAt = DateTime.UtcNow };
            mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<System.Threading.CancellationToken>())).Returns(true).Returns(false);
            mockCursor.Setup(x => x.Current).Returns(new[] { greeting });
            mockCollection.Setup(x => x.FindSync(
                It.IsAny<FilterDefinition<Greeting>>(),
                It.IsAny<FindOptions<Greeting, Greeting>>(),
                default)).Returns(mockCursor.Object);

            var repo = new GreetingMongoRepository("mongodb://localhost", "testdb");
            var field = typeof(GreetingMongoRepository).GetField("_collection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(field);
            field.SetValue(repo, mockCollection.Object);

            var result = await repo.GetLastGreetingAsync();
            Assert.NotNull(result);
            Assert.Equal("Hello", result.Message);
        }

        [Fact]
        public async Task SaveGreetingAsync_MockCollection_SavesGreeting()
        {
            var mockCollection = new Mock<IMongoCollection<Greeting>>();
                mockCollection.Setup(x => x.InsertOne(
                    It.IsAny<Greeting>(),
                    null,
                    default));

            var repo = new GreetingMongoRepository("mongodb://localhost", "testdb");
            var field = typeof(GreetingMongoRepository).GetField("_collection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(field);
            field.SetValue(repo, mockCollection.Object);

            var greeting = new Greeting { Id = 2, Message = "World", CreatedAt = DateTime.UtcNow };
            await repo.SaveGreetingAsync(greeting);
                mockCollection.Verify(x => x.InsertOne(greeting, null, default), Times.Once);
        }

        [Fact]
        public async Task GetGreetingByIdAsync_MockCollection_ReturnsGreeting()
        {
            var mockCollection = new Mock<IMongoCollection<Greeting>>();
            var mockCursor = new Mock<IAsyncCursor<Greeting>>();
            var greeting = new Greeting { Id = 3, Message = "Test", CreatedAt = DateTime.UtcNow };
            mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<System.Threading.CancellationToken>())).Returns(true).Returns(false);
            mockCursor.Setup(x => x.Current).Returns(new[] { greeting });
            mockCollection.Setup(x => x.FindSync(
                It.IsAny<FilterDefinition<Greeting>>(),
                It.IsAny<FindOptions<Greeting, Greeting>>(),
                default)).Returns(mockCursor.Object);

            var repo = new GreetingMongoRepository("mongodb://localhost", "testdb");
            var field = typeof(GreetingMongoRepository).GetField("_collection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(field);
            field.SetValue(repo, mockCollection.Object);

            var result = await repo.GetLastGreetingAsync();
            Assert.NotNull(result);
            Assert.Equal("Test", result.Message);
        }

        [Fact]
        public async Task GetGreetingsAsync_MockCollection_ReturnsPagedGreetings()
        {
            var mockCollection = new Mock<IMongoCollection<Greeting>>();
            var greetings = new[] {
                new Greeting { Id = 1, Message = "A", CreatedAt = DateTime.UtcNow },
                new Greeting { Id = 2, Message = "B", CreatedAt = DateTime.UtcNow }
            };
            var mockCursor = new Mock<IAsyncCursor<Greeting>>();
            mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<System.Threading.CancellationToken>())).Returns(true).Returns(false);
            mockCursor.Setup(x => x.Current).Returns(greetings);
            mockCollection.Setup(x => x.FindSync(
                It.IsAny<FilterDefinition<Greeting>>(),
                It.IsAny<FindOptions<Greeting, Greeting>>(),
                default)).Returns(mockCursor.Object);

            var repo = new GreetingMongoRepository("mongodb://localhost", "testdb");
            var field = typeof(GreetingMongoRepository).GetField("_collection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(field);
            field.SetValue(repo, mockCollection.Object);

            var result = await repo.GetGreetingsAsync(1, 2, "CreatedAt", false, null);
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllGreetingsAsync_MockCollection_ReturnsAllGreetings()
        {
            var mockCollection = new Mock<IMongoCollection<Greeting>>();
            var greetings = new[] {
                new Greeting { Id = 1, Message = "A", CreatedAt = DateTime.UtcNow },
                new Greeting { Id = 2, Message = "B", CreatedAt = DateTime.UtcNow }
            };
            var mockCursor = new Mock<IAsyncCursor<Greeting>>();
            mockCursor.SetupSequence(x => x.MoveNext(It.IsAny<System.Threading.CancellationToken>())).Returns(true).Returns(false);
            mockCursor.Setup(x => x.Current).Returns(greetings);
            mockCollection.Setup(x => x.FindSync(
                It.IsAny<FilterDefinition<Greeting>>(),
                It.IsAny<FindOptions<Greeting, Greeting>>(),
                default)).Returns(mockCursor.Object);

            var repo = new GreetingMongoRepository("mongodb://localhost", "testdb");
            var field = typeof(GreetingMongoRepository).GetField("_collection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(field);
            field.SetValue(repo, mockCollection.Object);

            var result = await repo.GetAllGreetingsAsync();
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }
    }
}
