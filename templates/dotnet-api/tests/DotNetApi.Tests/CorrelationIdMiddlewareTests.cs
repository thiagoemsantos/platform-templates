using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using DotNetApi.Middlewares;

namespace DotNetApi.Tests
{
    public class CorrelationIdMiddlewareTests
    {
        [Fact]
        public async Task Invoke_SetsCorrelationIdHeader()
        {
            var context = new DefaultHttpContext();
            var called = false;
            RequestDelegate next = ctx => { called = true; return Task.CompletedTask; };
            var middleware = new CorrelationIdMiddleware(next);
            await middleware.InvokeAsync(context);
            Assert.True(called);
            Assert.True(context.Response.Headers.ContainsKey("X-Correlation-Id") || context.Request.Headers.ContainsKey("X-Correlation-Id"));
        }
    }
}
