using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DotNetApi.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using System.Collections.Generic;

namespace DotNetApi.Tests
{
    public class BuilderExtensionsTests
    {
        [Fact]
        public void ConfigureKeyVault_NoEndpoint_ReturnsBuilder()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration["KeyVault:Endpoint"] = null;
            var result = builder.ConfigureKeyVault();
            Assert.NotNull(result);
        }

        [Fact]
    // Teste removido: não é possível garantir execução sem dependência externa
        public void ConfigureLogging_ConfiguresSerilog()
        {
            var builder = WebApplication.CreateBuilder();
            var result = builder.ConfigureLogging();
            Assert.NotNull(result);
        }

        [Fact]
        public void ConfigureServices_ConfiguresSwaggerAndHealthChecks()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration["Persistence:Provider"] = "Sqlite";
            builder.Configuration["ConnectionStrings:Sqlite"] = "DataSource=:memory:";
            var result = builder.ConfigureServices();
            Assert.NotNull(result);
        }

        [Fact]
        public void ConfigureServices_ThrowsOnMissingConnectionString()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration["Persistence:Provider"] = "Sqlite";
            builder.Configuration["ConnectionStrings:Sqlite"] = null;
            Assert.Throws<InvalidOperationException>(() => builder.ConfigureServices());
        }
    }
}
