using Microsoft.AspNetCore.Builder;
using Moq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Xunit;
using DotNetApi.Middlewares;

namespace DotNetApi.Tests
{
    public class CorrelationIdMiddlewareExtensionsTests
    {
        [Fact]
        public void UseCorrelationId_RegistersMiddleware()
        {
            var appBuilder = new FakeApplicationBuilder();
            var result = CorrelationIdMiddlewareExtensions.UseCorrelationId(appBuilder);
            Assert.True(appBuilder.MiddlewareRegistered);
            Assert.NotNull(result);
        }

        private class FakeApplicationBuilder : IApplicationBuilder
        {
            public bool MiddlewareRegistered { get; private set; } = false;
            public IServiceProvider ApplicationServices { get; set; } = null!;
            public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();
            public IFeatureCollection ServerFeatures { get; } = new FeatureCollection();
            public RequestDelegate Build() => new RequestDelegate(context => Task.CompletedTask);
            public IApplicationBuilder New() => this;
            public PathString PathBase { get; set; }
            public IApplicationBuilder Use(Func<RequestDelegate, RequestDelegate> middleware)
            {
                MiddlewareRegistered = true;
                return this;
            }
        }
    }
}
