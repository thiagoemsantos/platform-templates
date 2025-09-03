// Métodos de extensão para configuração
using Azure.Identity;
using DotNetApi.Application.Services;
using DotNetApi.Domain.Interfaces;
using DotNetApi.Infrastructure.Repositories;
using DotNetApi.Middlewares;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace DotNetApi.Extensions
{
    /// <summary>
    /// Métodos de extensão para configuração do builder e da aplicação.
    /// Mantém o Program.cs limpo e orquestrador.
    /// </summary>
    public static class BuilderExtensions
    {
        /// <summary>
        /// Configura o Azure Key Vault como fonte de configuração, se estiver definido.
        /// </summary>
        public static WebApplicationBuilder ConfigureKeyVault(
            this WebApplicationBuilder builder,
            Func<IConfigurationBuilder, string, IConfigurationBuilder>? addKeyVault = null)
        {
            var keyVaultEndpoint = builder.Configuration["KeyVault:Endpoint"];
            if (!string.IsNullOrEmpty(keyVaultEndpoint))
            {
                if (addKeyVault != null)
                {
                    addKeyVault(builder.Configuration as IConfigurationBuilder, keyVaultEndpoint);
                }
                else
                {
                    builder.Configuration.AddAzureKeyVault(
                        new Uri(keyVaultEndpoint),
                        new DefaultAzureCredential());
                }
            }
            return builder;
        }

        /// <summary>
        /// Configura o Serilog para logging estruturado.
        /// </summary>
        public static WebApplicationBuilder ConfigureLogging(this WebApplicationBuilder builder)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .CreateBootstrapLogger();

            builder.Host.UseSerilog((ctx, lc) => lc
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(ctx.Configuration));

            return builder;
        }

        /// <summary>
        /// Configura todos os serviços da aplicação, incluindo Swagger, HealthChecks, Observabilidade e Persistência.
        /// </summary>
        public static WebApplicationBuilder ConfigureServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "DotNetApi Platform Template", Version = "v1" });
                c.IncludeXmlComments(xmlPath, true);
            });

            // Health Checks dinâmicos conforme provider configurado
            var provider = builder.Configuration["Persistence:Provider"];
            var healthChecksBuilder = builder.Services.AddHealthChecks();

            switch (provider)
            {
                case "Sqlite":
                    var sqliteConnectionString = builder.Configuration.GetConnectionString("Sqlite");
                    if (string.IsNullOrEmpty(sqliteConnectionString))
                        throw new InvalidOperationException("Sqlite connection string is missing.");
                    healthChecksBuilder.AddSqlite(sqliteConnectionString, name: "sqlite");
                    break;

                case "MongoDB":
                    var mongoConnectionString = builder.Configuration.GetConnectionString("MongoDB");
                    if (string.IsNullOrEmpty(mongoConnectionString))
                        throw new InvalidOperationException("MongoDB connection string is missing.");
                    builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(mongoConnectionString));
                    healthChecksBuilder.AddMongoDb(
                        clientFactory: sp => sp.GetRequiredService<IMongoClient>(),
                        name: "mongodb",
                        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy
                    );
                    break;

                case "SqlServer":
                    var sqlServerConnectionString = builder.Configuration.GetConnectionString("SqlServer");
                    if (string.IsNullOrEmpty(sqlServerConnectionString))
                        throw new InvalidOperationException("SqlServer connection string is missing.");
                    healthChecksBuilder.AddSqlServer(sqlServerConnectionString, name: "sqlserver");
                    break;

                case "Oracle":
                    var oracleConnectionString = builder.Configuration.GetConnectionString("Oracle");
                    if (string.IsNullOrEmpty(oracleConnectionString))
                        throw new InvalidOperationException("Oracle connection string is missing.");
                    healthChecksBuilder.AddOracle(oracleConnectionString, name: "oracle");
                    break;

                default:
                    throw new InvalidOperationException($"Provider '{provider}' não é suportado para health checks.");
            }

            // Health check para API externa (exemplo)
            healthChecksBuilder.AddUrlGroup(
                new Uri("https://httpbin.org/status/200"),
                name: "api-externa",
                failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy);

            // Observabilidade (OpenTelemetry)
            var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            var serviceName = builder.Configuration["OTEL_SERVICE_NAME"]
                ?? (assemblyName != null ? assemblyName.ToLowerInvariant() : "defaultservicename");
            var serviceVersion = "1.0.0";
            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName, serviceVersion: serviceVersion))
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter())
                .WithMetrics(metrics => metrics
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(serviceName))
                    .AddAspNetCoreInstrumentation());

            // Persistência configurável
            builder.Services.AddScoped<IGreetingRepository>(sp =>
                GreetingRepositoryFactory.Create(
                    sp.GetRequiredService<IConfiguration>(),
                    sp.GetRequiredService<ILogger<GreetingSqlRepository>>(),
                    sp.GetRequiredService<IDistributedCache>()));
            builder.Services.Decorate<IGreetingRepository, GreetingRepositoryPollyDecorator>(); // Polly Decorator
            builder.Services.AddScoped<HelloService>();

            return builder;
        }

        /// <summary>
        /// Configura os middlewares da aplicação.
        /// </summary>
        public static WebApplication ConfigureMiddleware(this WebApplication app)
        {
            app.UseCorrelationId();
            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "DotNetApi Platform Template v1"));

            app.UseAuthorization();
            app.MapControllers();
            app.MapHealthChecks("/health");

            return app;
        }
    }
}