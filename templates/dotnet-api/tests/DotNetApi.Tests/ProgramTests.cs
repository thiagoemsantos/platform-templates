using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using DotNetApi.Extensions;

public class ProgramTests
{
    [Fact]
    public void Program_StartupPipeline_DoesNotThrow()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["KeyVault:Endpoint"] = ""; // Simula sem KeyVault
        builder.Configuration["Persistence:Provider"] = "Sqlite";
        builder.Configuration["ConnectionStrings:Sqlite"] = "DataSource=:memory:";
        var configuredBuilder = builder.ConfigureKeyVault().ConfigureLogging().ConfigureServices();
        var app = configuredBuilder.Build();
        Assert.NotNull(app);
    }
}
