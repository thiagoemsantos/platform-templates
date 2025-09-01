using System;
using System.Threading.Tasks;
using DotNetApi.Domain.Entities;
using DotNetApi.Infrastructure.Repositories;
using Moq;
using Xunit;

namespace DotNetApi.Tests
{
    public class GreetingRepositoryTests
    {
        private const string ConnectionString = "DataSource=:memory:;Cache=Shared";
        private Microsoft.Data.Sqlite.SqliteConnection CreateOpenConnectionWithTable()
        {
            var conn = new Microsoft.Data.Sqlite.SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "CREATE TABLE IF NOT EXISTS Greetings (Id INTEGER PRIMARY KEY AUTOINCREMENT, Message TEXT, CreatedAt TEXT);";
            cmd.ExecuteNonQuery();
            return conn;
        }

        [Fact]
        public async Task GetLastGreetingAsync_ReturnsNull_WhenTableEmpty()
        {
            using var conn = CreateOpenConnectionWithTable();
            var repo = new GreetingRepository(conn);
            var result = await repo.GetLastGreetingAsync();
            Assert.Null(result);
        }

        [Fact]
        public async Task SaveGreetingAsync_InsertsGreetingAndAssignsId()
        {
            using var conn = CreateOpenConnectionWithTable();
            var repo = new GreetingRepository(conn);
            var greeting = new Greeting { Message = "Hello" };
            await repo.SaveGreetingAsync(greeting);
            Assert.True(greeting.Id > 0);
        }

        [Fact]
        public async Task GetLastGreetingAsync_ReturnsGreeting_WhenExists()
        {
            using var conn = CreateOpenConnectionWithTable();
            var repo = new GreetingRepository(conn);
            var greeting = new Greeting { Message = "Hello" };
            await repo.SaveGreetingAsync(greeting);
            var result = await repo.GetLastGreetingAsync();
            Assert.NotNull(result);
            Assert.Equal("Hello", result!.Message);
        }

        [Fact]
        public async Task GetGreetingByIdAsync_ReturnsGreetingOrNull()
        {
            using var conn = CreateOpenConnectionWithTable();
            var repo = new GreetingRepository(conn);
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
            var repo = new GreetingRepository(conn);
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
            var repo = new GreetingRepository(conn);
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
            var repo = new GreetingRepository(conn);
            var result = await repo.GetGreetingsAsync(1, 10, "CreatedAt", true, "Nada");
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllGreetingsAsync_ReturnsAllOrEmpty()
        {
            using var conn = CreateOpenConnectionWithTable();
            var repo = new GreetingRepository(conn);
            var empty = await repo.GetAllGreetingsAsync();
            Assert.Empty(empty);
            await repo.SaveGreetingAsync(new Greeting { Message = "X" });
            var all = await repo.GetAllGreetingsAsync();
            Assert.Single(all);
        }
    }
}
