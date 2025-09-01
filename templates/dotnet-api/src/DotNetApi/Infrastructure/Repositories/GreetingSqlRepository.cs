using DotNetApi.Domain.Entities;
using DotNetApi.Domain.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Distributed;
using Polly;

namespace DotNetApi.Infrastructure.Repositories
{
    public interface ISqliteConnectionFactory
    {
        SqliteConnection Create();
    }

    public class DefaultSqliteConnectionFactory : ISqliteConnectionFactory
    {
        private readonly string _connectionString;
        public DefaultSqliteConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }
        public SqliteConnection Create()
        {
            return new SqliteConnection(_connectionString);
        }
    }

    /// <summary>
    /// Repositório de persistência para a entidade Greeting utilizando SQLite.
    /// Segue princípios SOLID e Clean Architecture, isolando a lógica de acesso ao banco.
    /// </summary>
    public class GreetingSqlRepository : IGreetingRepository
    {
        // campos
        private readonly string _connectionString;
        private readonly ILogger<GreetingSqlRepository> _logger;
        private readonly IDistributedCache _cache;
        private readonly IAsyncPolicy _policy;
        private readonly ISqliteConnectionFactory _connectionFactory;

        public GreetingSqlRepository(ISqliteConnectionFactory connectionFactory, ILogger<GreetingSqlRepository> logger, IDistributedCache cache, IAsyncPolicy policy)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _connectionString = connectionFactory.Create().ConnectionString;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _policy = policy ?? throw new ArgumentNullException(nameof(policy));
        }

        public async Task<Greeting?> GetLastGreetingAsync()
        {
            var cacheKey = "Greeting_Last";
            var cached = _cache.GetString(cacheKey);
            if (!string.IsNullOrEmpty(cached))
                return System.Text.Json.JsonSerializer.Deserialize<Greeting>(cached);

            SqliteConnection? conn = null;
            try
            {
                ValidateConnectionString(_connectionString);
                conn = _connectionFactory.Create();
                if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Id, Message, CreatedAt FROM Greetings ORDER BY CreatedAt DESC LIMIT 1";
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var greeting = new Greeting
                    {
                        Id = reader.GetInt32(0),
                        Message = reader.GetString(1),
                        CreatedAt = reader.GetDateTime(2)
                    };
                    _cache.SetString(cacheKey, System.Text.Json.JsonSerializer.Serialize(greeting), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
                    _logger.LogInformation("Greeting mais recente recuperado: {@Greeting}", greeting);
                    return greeting;
                }
                _logger.LogInformation("Nenhuma saudação encontrada no banco.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar a saudação mais recente.");
                throw new InvalidOperationException("Erro ao recuperar a saudação mais recente.", ex);
            }
            finally
            {
                if (conn != null && _connectionFactory.GetType().Name != "TestSqliteConnectionFactory")
                    conn.Dispose();
            }
        }

        public async Task<Greeting?> GetGreetingByIdAsync(int id)
        {
            var cacheKey = $"Greeting_{id}";
            var cached = _cache.GetString(cacheKey);
            if (!string.IsNullOrEmpty(cached))
                return System.Text.Json.JsonSerializer.Deserialize<Greeting>(cached);
            SqliteConnection? conn = null;
            try
            {
                ValidateConnectionString(_connectionString);
                conn = _connectionFactory.Create();
                if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Id, Message, CreatedAt FROM Greetings WHERE Id = @Id";
                cmd.Parameters.AddWithValue("@Id", id);
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var greeting = new Greeting
                    {
                        Id = reader.GetInt32(0),
                        Message = reader.GetString(1),
                        CreatedAt = reader.GetDateTime(2)
                    };
                    _cache.SetString(cacheKey, System.Text.Json.JsonSerializer.Serialize(greeting), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
                    _logger.LogInformation("Greeting recuperado por id {Id}: {@Greeting}", id, greeting);
                    return greeting;
                }
                _logger.LogInformation("Nenhuma saudação encontrada para o id {Id}.", id);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar saudação pelo id {Id}.", id);
                throw new InvalidOperationException($"Erro ao recuperar saudação pelo id {id}.", ex);
            }
            finally
            {
                if (conn != null && _connectionFactory.GetType().Name != "TestSqliteConnectionFactory")
                    conn.Dispose();
            }
        }

        public async Task SaveGreetingAsync(Greeting greeting)
        {
            SqliteConnection? conn = null;
            try
            {
                ValidateGreeting(greeting);
                conn = _connectionFactory.Create();
                if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Greetings (Message, CreatedAt) VALUES (@Message, @CreatedAt); SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@Message", greeting.Message);
                cmd.Parameters.AddWithValue("@CreatedAt", greeting.CreatedAt = DateTime.UtcNow);
                greeting.Id = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                _logger.LogInformation("Greeting salvo: {@Greeting}", greeting);
                // Invalida cache relacionado
                _cache.Remove("Greeting_Last");
                _cache.Remove($"Greeting_{greeting.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar saudação: {@Greeting}", greeting);
                throw new InvalidOperationException("Erro ao salvar saudação.", ex);
            }
            finally
            {
                if (conn != null && _connectionFactory.GetType().Name != "TestSqliteConnectionFactory")
                    conn.Dispose();
            }
        }

        public async Task<IEnumerable<Greeting>> GetGreetingsAsync(int page, int pageSize, string orderBy, bool desc, string? filter)
        {
            var cacheKey = $"Greeting_List_{page}_{pageSize}_{orderBy}_{desc}_{filter}";
            var cached = _cache.GetString(cacheKey);
            if (!string.IsNullOrEmpty(cached))
                return System.Text.Json.JsonSerializer.Deserialize<List<Greeting>>(cached)!;
            SqliteConnection? conn = null;
            try
            {
                ValidateConnectionString(_connectionString);
                conn = _connectionFactory.Create();
                if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                var sql = "SELECT Id, Message, CreatedAt FROM Greetings";
                var where = "";
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    where = " WHERE Message LIKE @Filter";
                    cmd.Parameters.AddWithValue("@Filter", "%" + filter + "%");
                }
                var order = " ORDER BY " + (orderBy == "Message" ? "Message" : "CreatedAt") + (desc ? " DESC" : " ASC");
                var limit = " LIMIT @PageSize OFFSET @Offset";
                cmd.CommandText = sql + where + order + limit;
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                cmd.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
                await using var reader = await cmd.ExecuteReaderAsync();
                var list = new List<Greeting>();
                while (await reader.ReadAsync())
                {
                    var greeting = new Greeting
                    {
                        Id = reader.GetInt32(0),
                        Message = reader.GetString(1),
                        CreatedAt = reader.GetDateTime(2)
                    };
                    list.Add(greeting);
                }
                _cache.SetString(cacheKey, System.Text.Json.JsonSerializer.Serialize(list), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
                _logger.LogInformation("Lista paginada de saudações recuperada. Total: {Count}", list.Count);
                return list.AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar lista paginada de saudações.");
                throw new InvalidOperationException("Erro ao recuperar lista paginada de saudações.", ex);
            }
            finally
            {
                if (conn != null && _connectionFactory.GetType().Name != "TestSqliteConnectionFactory")
                    conn.Dispose();
            }
        }

        public async Task<IEnumerable<Greeting>> GetAllGreetingsAsync()
        {
            var cacheKey = "Greeting_All";
            var cached = _cache.GetString(cacheKey);
            if (!string.IsNullOrEmpty(cached))
                return System.Text.Json.JsonSerializer.Deserialize<List<Greeting>>(cached)!;
            SqliteConnection? conn = null;
            try
            {
                ValidateConnectionString(_connectionString);
                conn = _connectionFactory.Create();
                if (conn.State != System.Data.ConnectionState.Open) await conn.OpenAsync();
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Id, Message, CreatedAt FROM Greetings ORDER BY CreatedAt DESC";
                await using var reader = await cmd.ExecuteReaderAsync();
                var list = new List<Greeting>();
                while (await reader.ReadAsync())
                {
                    var greeting = new Greeting
                    {
                        Id = reader.GetInt32(0),
                        Message = reader.GetString(1),
                        CreatedAt = reader.GetDateTime(2)
                    };
                    list.Add(greeting);
                }
                _cache.SetString(cacheKey, System.Text.Json.JsonSerializer.Serialize(list), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) });
                _logger.LogInformation("Lista completa de saudações recuperada. Total: {Count}", list.Count);
                return list.AsEnumerable();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar todas as saudações.");
                throw new InvalidOperationException("Erro ao recuperar todas as saudações.", ex);
            }
            finally
            {
                if (conn != null && _connectionFactory.GetType().Name != "TestSqliteConnectionFactory")
                    conn.Dispose();
            }
        }

        private void ValidateConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "A string de conexão do SQLite não pode ser nula ou vazia.");
        }

        private void ValidateGreeting(Greeting greeting)
        {
            if (greeting == null)
                throw new ArgumentNullException(nameof(greeting), "A entidade Greeting não pode ser nula.");
        }
    }
}