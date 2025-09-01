using DotNetApi.Extensions;

var builder = WebApplication.CreateBuilder(args)
    .ConfigureKeyVault()
    .ConfigureLogging()
    .ConfigureServices();

var app = builder.Build().ConfigureMiddleware();
app.Run();

public partial class Program { }