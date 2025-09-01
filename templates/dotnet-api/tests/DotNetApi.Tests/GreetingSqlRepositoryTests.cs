using System;
using System.Threading.Tasks;
using DotNetApi.Domain.Entities;
using DotNetApi.Infrastructure.Repositories;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace DotNetApi.Tests
{
    // Factory auxiliar para testes, retorna conexão já aberta
    public class TestSqliteConnectionFactory : ISqliteConnectionFactory
    {
        private readonly Microsoft.Data.Sqlite.SqliteConnection _connection;
        public TestSqliteConnectionFactory(Microsoft.Data.Sqlite.SqliteConnection connection)
        {
            _connection = connection;
        }
        public Microsoft.Data.Sqlite.SqliteConnection Create() => _connection;
    }
    public class GreetingSqlRepositoryTests
    {
        private const string ConnectionString = "DataSource=:memory:";

        private Microsoft.Data.Sqlite.SqliteConnection CreateOpenConnectionWithTable()
        {
            var conn = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS Greetings (Id INTEGER PRIMARY KEY AUTOINCREMENT, Message TEXT, CreatedAt TEXT);";
            cmd.ExecuteNonQuery();
            return conn;
        }

        private GreetingSqlRepository CreateRepository(Mock<IDistributedCache> cacheMock, Mock<ILogger<GreetingSqlRepository>> loggerMock)
        {
            var connectionFactory = new DefaultSqliteConnectionFactory(ConnectionString);
            var policy = Polly.Policy.NoOpAsync();
            return new GreetingSqlRepository(connectionFactory, loggerMock.Object, cacheMock.Object, policy);
        }

        [Fact]
        public async Task SaveGreetingAsync_InsertsGreetingAndAssignsId()
        {
            using var conn = CreateOpenConnectionWithTable();
            var cacheMock = new Mock<IDistributedCache>();
            var loggerMock = new Mock<ILogger<GreetingSqlRepository>>();
            var connectionFactory = new TestSqliteConnectionFactory(conn);
            var policy = Polly.Policy.NoOpAsync();
            var repo = new GreetingSqlRepository(connectionFactory, loggerMock.Object, cacheMock.Object, policy);
            var greeting = new Greeting { Message = "Hello" };
            await repo.SaveGreetingAsync(greeting);
            Assert.True(greeting.Id > 0);
        }

        [Fact]
        public async Task GetLastGreetingAsync_ReturnsGreetingOrNull()
        {
            using var conn = CreateOpenConnectionWithTable();
            var cacheMock = new Mock<IDistributedCache>();
            var loggerMock = new Mock<ILogger<GreetingSqlRepository>>();
            var connectionFactory = new TestSqliteConnectionFactory(conn);
            var policy = Polly.Policy.NoOpAsync();
            var repo = new GreetingSqlRepository(connectionFactory, loggerMock.Object, cacheMock.Object, policy);
            var greeting = new Greeting { Message = "Test" };
            await repo.SaveGreetingAsync(greeting);
            var result = await repo.GetLastGreetingAsync();
            Assert.NotNull(result);
            Assert.Equal("Test", result!.Message);
            // Testa branch sem dados
            using var conn2 = CreateOpenConnectionWithTable();
            var connectionFactory2 = new TestSqliteConnectionFactory(conn2);
            var policy2 = Polly.Policy.NoOpAsync();
            var repo2 = new GreetingSqlRepository(connectionFactory2, loggerMock.Object, cacheMock.Object, policy2);
            var empty = await repo2.GetLastGreetingAsync();
            Assert.Null(empty);
        }

        [Fact]
        public async Task GetGreetingByIdAsync_ReturnsGreetingOrNull()
        {
            using var conn = CreateOpenConnectionWithTable();
            var cacheMock = new Mock<IDistributedCache>();
            var loggerMock = new Mock<ILogger<GreetingSqlRepository>>();
            var connectionFactory = new TestSqliteConnectionFactory(conn);
            var policy = Polly.Policy.NoOpAsync();
            var repo = new GreetingSqlRepository(connectionFactory, loggerMock.Object, cacheMock.Object, policy);
            var greeting = new Greeting { Message = "Test" };
            await repo.SaveGreetingAsync(greeting);
            var found = await repo.GetGreetingByIdAsync(greeting.Id);
            Assert.NotNull(found);
            Assert.Equal("Test", found!.Message);
            var notFound = await repo.GetGreetingByIdAsync(999);
            Assert.Null(notFound);
        }

        [Fact]
        public async Task GetGreetingsAsync_ReturnsPagedAndFiltered()
        {
            using var conn = CreateOpenConnectionWithTable();
            var cacheMock = new Mock<IDistributedCache>();
            var loggerMock = new Mock<ILogger<GreetingSqlRepository>>();
            var connectionFactory = new TestSqliteConnectionFactory(conn);
            var policy = Polly.Policy.NoOpAsync();
            var repo = new GreetingSqlRepository(connectionFactory, loggerMock.Object, cacheMock.Object, policy);
            for (int i = 1; i <= 5; i++)
                await repo.SaveGreetingAsync(new Greeting { Message = $"Msg{i}" });
            var page1 = await repo.GetGreetingsAsync(1, 2, "CreatedAt", true, null);
            Assert.Equal(2, page1.Count());
            var filtered = await repo.GetGreetingsAsync(1, 10, "CreatedAt", true, "Msg3");
            Assert.Single(filtered);
            Assert.Equal("Msg3", filtered.First().Message);
        }

        [Fact]
        public async Task GetGreetingsAsync_OrderingAscDesc()
        {
            using var conn = CreateOpenConnectionWithTable();
            var cacheMock = new Mock<IDistributedCache>();
            var loggerMock = new Mock<ILogger<GreetingSqlRepository>>();
            var connectionFactory = new TestSqliteConnectionFactory(conn);
            var policy = Polly.Policy.NoOpAsync();
            var repo = new GreetingSqlRepository(connectionFactory, loggerMock.Object, cacheMock.Object, policy);
            await repo.SaveGreetingAsync(new Greeting { Message = "A" });
            await repo.SaveGreetingAsync(new Greeting { Message = "B" });
            var asc = await repo.GetGreetingsAsync(1, 10, "Message", false, null);
            Assert.Equal("A", asc.First().Message);
            var desc = await repo.GetGreetingsAsync(1, 10, "Message", true, null);
            Assert.Equal("B", desc.First().Message);
        }

        [Fact]
        public async Task GetGreetingsAsync_ReturnsEmpty_WhenNoMatch()
        {
            using var conn = CreateOpenConnectionWithTable();
            var cacheMock = new Mock<IDistributedCache>();
            var loggerMock = new Mock<ILogger<GreetingSqlRepository>>();
            var connectionFactory = new TestSqliteConnectionFactory(conn);
            var policy = Polly.Policy.NoOpAsync();
            var repo = new GreetingSqlRepository(connectionFactory, loggerMock.Object, cacheMock.Object, policy);
            var result = await repo.GetGreetingsAsync(1, 10, "CreatedAt", true, "Nada");
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllGreetingsAsync_ReturnsAllOrEmpty()
        {
            using var conn = CreateOpenConnectionWithTable();
            var cacheMock = new Mock<IDistributedCache>();
            var loggerMock = new Mock<ILogger<GreetingSqlRepository>>();
            var connectionFactory = new TestSqliteConnectionFactory(conn);
            var policy = Polly.Policy.NoOpAsync();
            var repo = new GreetingSqlRepository(connectionFactory, loggerMock.Object, cacheMock.Object, policy);
            var empty = await repo.GetAllGreetingsAsync();
            Assert.Empty(empty);
            await repo.SaveGreetingAsync(new Greeting { Message = "X" });
            var all = await repo.GetAllGreetingsAsync();
            Assert.Single(all);
        }
    
        [Fact]
        public void Constructor_ThrowsOnNullConnectionString()
        {
            var logger = new Mock<ILogger<GreetingSqlRepository>>().Object;
            var cache = new Mock<IDistributedCache>().Object;
            var policy = Polly.Policy.NoOpAsync();
            Assert.Throws<ArgumentNullException>(() => new GreetingSqlRepository(null!, logger, cache, policy));
        }

        [Fact]
        public void Constructor_ThrowsOnNullLogger()
        {
            var cache = new Mock<IDistributedCache>().Object;
            var connectionFactory = new DefaultSqliteConnectionFactory("DataSource=:memory:");
            var policy = Polly.Policy.NoOpAsync();
            Assert.Throws<ArgumentNullException>(() => new GreetingSqlRepository(connectionFactory, null!, cache, policy));
        }

        [Fact]
        public void Constructor_ThrowsOnNullCache()
        {
            var logger = new Mock<ILogger<GreetingSqlRepository>>().Object;
            var connectionFactory = new DefaultSqliteConnectionFactory("DataSource=:memory:");
            var policy = Polly.Policy.NoOpAsync();
            Assert.Throws<ArgumentNullException>(() => new GreetingSqlRepository(connectionFactory, logger, null!, policy));
        }

    }
}
