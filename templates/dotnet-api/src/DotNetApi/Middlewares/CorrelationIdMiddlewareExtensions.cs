namespace DotNetApi.Middlewares
{
    /// <summary>
    /// Extensão para registrar o CorrelationIdMiddleware na pipeline de requisições.
    /// Facilita a configuração do middleware seguindo boas práticas de Clean Architecture.
    /// </summary>
    public static class CorrelationIdMiddlewareExtensions
    {
        /// <summary>
        /// Adiciona o CorrelationIdMiddleware à pipeline de requisições.
        /// </summary>
        /// <param name="builder">Application builder do ASP.NET Core.</param>
        /// <returns>Application builder para encadeamento.</returns>
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationIdMiddleware>();
        }
    }
}