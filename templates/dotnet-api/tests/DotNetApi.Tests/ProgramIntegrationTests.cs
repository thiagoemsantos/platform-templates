using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DotNetApi.Tests
{
    public class ProgramIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public ProgramIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetSwagger_ReturnsOk()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/swagger/index.html");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task CorrelationIdMiddleware_AddsHeader()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/api/hello");
            Assert.True(response.Headers.Contains("X-Correlation-Id"));
        }
    }
}
