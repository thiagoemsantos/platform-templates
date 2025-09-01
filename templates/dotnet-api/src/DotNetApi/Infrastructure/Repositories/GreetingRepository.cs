using DotNetApi.Domain.Entities;
using DotNetApi.Domain.Interfaces;
using Microsoft.Data.Sqlite;

namespace DotNetApi.Infrastructure.Repositories
{
    /// <summary>
    /// Repositório para a entidade Greeting.
    /// </summary>
    public class GreetingRepository : IGreetingRepository
    {
    private readonly string _connectionString;
    private readonly SqliteConnection? _testConnection;

        /// <summary>
        /// Construtor da classe GreetingRepository.
        /// </summary>
        /// <param name="connectionString">String de conexão com o banco de dados.</param>
        public GreetingRepository(string connectionString)
        {
            _connectionString = connectionString;
            _testConnection = null;
        }

        // Construtor para testes, aceita uma conexão já aberta
        public GreetingRepository(SqliteConnection testConnection)
        {
            _connectionString = testConnection.ConnectionString;
            _testConnection = testConnection;
        }

        /// <summary>
        /// Recupera a última mensagem Greeting do banco de dados.
        /// </summary>
        /// <returns>Entidade Greeting com a mensagem mais recente.</returns>
    public async Task<Greeting?> GetLastGreetingAsync()
        {
            return await Task.Run(() => {
                var conn = _testConnection ?? new SqliteConnection(_connectionString);
                if (_testConnection == null) conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Id, Message, CreatedAt FROM Greetings ORDER BY CreatedAt DESC LIMIT 1";
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Greeting
                    {
                        Id = reader.GetInt32(0),
                        Message = reader.GetString(1),
                        CreatedAt = reader.GetDateTime(2)
                    };
                }
                return null!;
            });
        }

        /// <summary>
        /// Recupera uma mensagem Greeting pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador da mensagem Greeting.</param>
        /// <returns>Entidade Greeting correspondente ao identificador fornecido.</returns>
        public async Task<Greeting?> GetGreetingByIdAsync(int id)
        {
            return await Task.Run(() => {
                var conn = _testConnection ?? new SqliteConnection(_connectionString);
                if (_testConnection == null) conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Id, Message, CreatedAt FROM Greetings WHERE Id = @Id";
                cmd.Parameters.AddWithValue("@Id", id);
                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new Greeting
                    {
                        Id = reader.GetInt32(0),
                        Message = reader.GetString(1),
                        CreatedAt = reader.GetDateTime(2)
                    };
                }
                return null!;
            });
        }

        /// <summary>
        /// Salva uma nova entidade Greeting no banco de dados.
        /// </summary>
        /// <param name="greeting">Entidade Greeting a ser salva.</param>
        public async Task SaveGreetingAsync(Greeting greeting)
        {
            await Task.Run(() => {
                var conn = _testConnection ?? new SqliteConnection(_connectionString);
                if (_testConnection == null) conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO Greetings (Message, CreatedAt) VALUES (@Message, @CreatedAt); SELECT last_insert_rowid();";
                cmd.Parameters.AddWithValue("@Message", greeting.Message);
                cmd.Parameters.AddWithValue("@CreatedAt", greeting.CreatedAt = DateTime.UtcNow);
                greeting.Id = Convert.ToInt32(cmd.ExecuteScalar());
            });
        }

        /// <summary>
        /// Recupera saudações paginadas, ordenadas e filtradas do SQLite.
        /// </summary>
        public async Task<IEnumerable<Greeting>> GetGreetingsAsync(int page, int pageSize, string orderBy, bool desc, string? filter)
        {
            return await Task.Run(() => {
                var conn = _testConnection ?? new SqliteConnection(_connectionString);
                if (_testConnection == null) conn.Open();
                using var cmd = conn.CreateCommand();
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
                using var reader = cmd.ExecuteReader();
                var list = new List<Greeting>();
                while (reader.Read())
                {
                    list.Add(new Greeting
                    {
                        Id = reader.GetInt32(0),
                        Message = reader.GetString(1),
                        CreatedAt = reader.GetDateTime(2)
                    });
                }
                return list.AsEnumerable();
            });
        }
        /// <summary>
        /// Recupera todas as saudações cadastradas.
        /// </summary>
        public async Task<IEnumerable<Greeting>> GetAllGreetingsAsync()
        {
            return await Task.Run(() => {
                var conn = _testConnection ?? new SqliteConnection(_connectionString);
                if (_testConnection == null) conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Id, Message, CreatedAt FROM Greetings ORDER BY CreatedAt DESC";
                using var reader = cmd.ExecuteReader();
                var list = new List<Greeting>();
                while (reader.Read())
                {
                    list.Add(new Greeting
                    {
                        Id = reader.GetInt32(0),
                        Message = reader.GetString(1),
                        CreatedAt = reader.GetDateTime(2)
                    });
                }
                return list.AsEnumerable();
            });
        }
    }
}