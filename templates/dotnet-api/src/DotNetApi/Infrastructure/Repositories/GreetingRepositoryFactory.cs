using DotNetApi.Domain.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace DotNetApi.Infrastructure.Repositories
{
    /// <summary>
    /// Fábrica responsável por criar instâncias de IGreetingRepository com base na configuração.
    /// Segue princípios SOLID e Clean Architecture ao abstrair a lógica de criação dos repositórios.
    /// </summary>
    public static class GreetingRepositoryFactory
    {
        /// <summary>
        /// Cria uma instância única de IGreetingRepository conforme o provider configurado.
        /// </summary>
    /// <param name="config">Configuração da aplicação.</param>
    /// <param name="logger">Logger para o repositório SQL.</param>
    /// <param name="cache">Cache distribuído para o repositório SQL.</param>
    /// <returns>Implementação concreta de IGreetingRepository.</returns>
    /// <exception cref="ArgumentNullException">Se o provider não for especificado.</exception>
    /// <exception cref="NotSupportedException">Se o provider não for suportado.</exception>
        public static IGreetingRepository Create(IConfiguration config, ILogger<GreetingSqlRepository> logger, IDistributedCache cache)
        {
            var provider = config["Persistence:Provider"];
            return CreateForProviderOrThrow(config, provider, logger, cache);
        }

        public static IGreetingRepository CreateForProviderOrThrow(IConfiguration config, string? provider, ILogger<GreetingSqlRepository> logger, IDistributedCache cache)
        {
            if (provider is null)
                throw new ArgumentNullException(nameof(provider));

            return provider switch
            {
                "Sqlite" => CreateSqliteRepository(config, logger, cache),
                "MongoDB" => CreateMongoRepository(config),
                _ => throw new NotSupportedException($"Provider '{provider}' não é suportado.")
            };
        }

        private static void ValidateProvider(string provider)
        {
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentNullException(nameof(provider), "O provedor de persistência deve ser especificado.");
        }

        private static IGreetingRepository CreateSqliteRepository(IConfiguration config, ILogger<GreetingSqlRepository> logger, IDistributedCache cache)
        {
            var connStr = config.GetConnectionString("Sqlite");
            if (string.IsNullOrEmpty(connStr))
                throw new ArgumentNullException(nameof(connStr), "A string de conexão do SQLite não pode ser nula ou vazia.");

            var connectionFactory = new DefaultSqliteConnectionFactory(connStr);
            var policy = Polly.Policy.NoOpAsync();
            return new GreetingSqlRepository(connectionFactory, logger, cache, policy);
        }

        private static IGreetingRepository CreateMongoRepository(IConfiguration config)
        {
            var mongoConnStr = config.GetConnectionString("MongoDB");
            if (string.IsNullOrEmpty(mongoConnStr))
                throw new ArgumentNullException(nameof(mongoConnStr), "A string de conexão do MongoDB não pode ser nula ou vazia.");

            var dbName = config["Persistence:MongoDbName"];
            if (string.IsNullOrEmpty(dbName))
                throw new ArgumentNullException(nameof(dbName), "O nome do banco de dados do MongoDB não pode ser nulo ou vazio.");

            return new GreetingMongoRepository(mongoConnStr, dbName);
        }
    }
}