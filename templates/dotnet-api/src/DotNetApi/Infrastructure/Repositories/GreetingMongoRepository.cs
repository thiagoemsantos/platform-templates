using DotNetApi.Domain.Entities;
using DotNetApi.Domain.Interfaces;
using MongoDB.Driver;

namespace DotNetApi.Infrastructure.Repositories
{
    /// <summary>
    /// Repositório de persistência para a entidade Greeting utilizando MongoDB.
    /// Segue princípios SOLID e Clean Architecture, isolando a lógica de acesso ao banco.
    /// </summary>
    public class GreetingMongoRepository : IGreetingRepository
    {
        private readonly IMongoCollection<Greeting> _collection;

        /// <summary>
        /// Inicializa o repositório MongoDB com a string de conexão e nome do banco.
        /// </summary>
        /// <param name="connectionString">String de conexão do MongoDB.</param>
        /// <param name="dbName">Nome do banco de dados.</param>
        /// <exception cref="ArgumentNullException">Se a string de conexão ou nome do banco forem nulos ou vazios.</exception>
        public GreetingMongoRepository(string connectionString, string dbName)
        {
            ValidateConnectionParameters(connectionString, dbName);

            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(dbName);
            _collection = database.GetCollection<Greeting>("Greetings");
        }

        /// <summary>
        /// Recupera o Greeting mais recente do MongoDB.
        /// </summary>
        /// <returns>Entidade Greeting ou null se não houver registros.</returns>
    public async Task<Greeting?> GetLastGreetingAsync()
        {
            // Consulta técnica, já encapsulada.
            return await Task.Run(() => _collection.Find(_ => true).SortByDescending(g => g.CreatedAt).FirstOrDefault());
        }

        /// <summary>
        /// Persiste uma nova entidade Greeting no MongoDB.
        /// </summary>
        /// <param name="greeting">Entidade Greeting a ser salva.</param>
        /// <exception cref="ArgumentNullException">Se a entidade Greeting for nula.</exception>
        public async Task SaveGreetingAsync(Greeting greeting)
        {
            ValidateGreeting(greeting);
            await Task.Run(() => _collection.InsertOne(greeting));
        }

        /// <summary>
        /// Recupera uma entidade Greeting pelo Id.
        /// </summary>
        /// <param name="id">Id da saudação.</param>
        /// <returns>Entidade Greeting ou null se não encontrada.</returns>
        public async Task<Greeting?> GetGreetingByIdAsync(int id)
        {
            return await Task.Run(() => _collection.Find(g => g.Id == id).FirstOrDefault());
        }

        /// <summary>
        /// Recupera saudações paginadas, ordenadas e filtradas do MongoDB.
        /// </summary>
        public async Task<IEnumerable<Greeting>> GetGreetingsAsync(int page, int pageSize, string orderBy, bool desc, string? filter)
        {
            return await Task.Run(() => {
                var filterDef = Builders<Greeting>.Filter.Empty;
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    filterDef = Builders<Greeting>.Filter.Regex("Message", new MongoDB.Bson.BsonRegularExpression(filter, "i"));
                }
                var sortDef = desc
                    ? Builders<Greeting>.Sort.Descending(orderBy == "Message" ? "Message" : "CreatedAt")
                    : Builders<Greeting>.Sort.Ascending(orderBy == "Message" ? "Message" : "CreatedAt");
                return _collection.Find(filterDef)
                    .Sort(sortDef)
                    .Skip((page - 1) * pageSize)
                    .Limit(pageSize)
                    .ToList().AsEnumerable();
            });
        }

        /// <summary>
        /// Recupera todas as saudações cadastradas.
        /// </summary>
        public async Task<IEnumerable<Greeting>> GetAllGreetingsAsync()
        {
            return await Task.Run(() => _collection.Find(_ => true).SortByDescending(g => g.CreatedAt).ToList().AsEnumerable());
        }

        /// <summary>
        /// Regra de negócio: valida os parâmetros de conexão.
        /// </summary>
        private void ValidateConnectionParameters(string connectionString, string dbName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentNullException(nameof(connectionString), "A string de conexão do MongoDB não pode ser nula ou vazia.");
            if (string.IsNullOrWhiteSpace(dbName))
                throw new ArgumentNullException(nameof(dbName), "O nome do banco de dados do MongoDB não pode ser nulo ou vazio.");
        }

        /// <summary>
        /// Regra de negócio: valida a entidade Greeting.
        /// </summary>
        private void ValidateGreeting(Greeting greeting)
        {
            if (greeting == null)
                throw new ArgumentNullException(nameof(greeting), "A entidade Greeting não pode ser nula.");
        }
    }
}