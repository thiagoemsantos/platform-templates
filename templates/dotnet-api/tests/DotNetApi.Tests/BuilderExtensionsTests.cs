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
            // ...existing code...
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
        public void ConfigureKeyVault_WithEndpoint_AddsKeyVault()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration["KeyVault:Endpoint"] = "https://fakevault.vault.azure.net/";

            bool called = false;
            var result = builder.ConfigureKeyVault((configBuilder, endpoint) => {
                called = true;
                // Simula adição do KeyVault sem dependência externa
                return configBuilder;
            });
            Assert.NotNull(result);
            Assert.True(called, "O delegate de KeyVault foi chamado.");
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

        [Theory]
        [InlineData("Sqlite")]
        [InlineData("MongoDB")]
        [InlineData("SqlServer")]
        [InlineData("Oracle")]
        public void ConfigureServices_ValidProviders_ReturnsBuilder(string provider)
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration["Persistence:Provider"] = provider;
            builder.Configuration[$"ConnectionStrings:{provider}"] = "fake-connection-string";
            var result = builder.ConfigureServices();
            Assert.NotNull(result);
        }

        [Fact]
        public void ConfigureServices_InvalidProvider_Throws()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration["Persistence:Provider"] = "InvalidProvider";
            Assert.Throws<InvalidOperationException>(() => builder.ConfigureServices());
        }

        [Fact]
        public void ConfigureMiddleware_ReturnsApp()
        {
            var builder = WebApplication.CreateBuilder();
            builder.Configuration["Persistence:Provider"] = "Sqlite";
            builder.Configuration["ConnectionStrings:Sqlite"] = "fake-connection-string";
            var app = builder.ConfigureServices().Build();
            var result = app.ConfigureMiddleware();
            Assert.NotNull(result);
        }
    }
}
