using Serilog;

namespace DotNetApi.Middlewares
{
    /// <summary>
    /// Middleware responsável por garantir e registrar o CorrelationId nas requisições HTTP.
    /// Segue boas práticas de rastreabilidade e logging, enriquecendo os logs com informações relevantes.
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Inicializa o middleware com o próximo delegate da pipeline.
        /// </summary>
        /// <param name="next">Delegate da próxima etapa do pipeline.</param>
        public CorrelationIdMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        /// <summary>
        /// Executa a lógica do middleware: garante o CorrelationId, adiciona ao contexto e enriquece os logs.
        /// </summary>
        /// <param name="context">Contexto da requisição HTTP.</param>
        public async Task InvokeAsync(HttpContext context)
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();
            context.Items["CorrelationId"] = correlationId;
            context.Response.Headers["X-Correlation-ID"] = correlationId;

            Log.ForContext("CorrelationId", correlationId)
                .ForContext("UserAgent", context.Request.Headers["User-Agent"].ToString())
                .ForContext("RemoteIp", context.Connection.RemoteIpAddress?.ToString())
                .Information("Request {Method} {Path} from {RemoteIp} UA:{UserAgent}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Connection.RemoteIpAddress,
                    context.Request.Headers["User-Agent"].ToString());

            await _next(context);
        }
    }
}