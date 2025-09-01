using DotNetApi.Domain.Entities;
using DotNetApi.Domain.Interfaces;
using Polly;

namespace DotNetApi.Infrastructure.Repositories
{
    /// <summary>
    /// Decorador para IGreetingRepository que aplica políticas Polly de resiliência.
    /// </summary>
    public class GreetingRepositoryPollyDecorator : IGreetingRepository
    {
        private readonly IGreetingRepository _inner;
        private readonly AsyncPolicy _policy;

        public GreetingRepositoryPollyDecorator(IGreetingRepository inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _policy = Policy.WrapAsync(
                Policy.Handle<Exception>().WaitAndRetryAsync(3, retry => TimeSpan.FromMilliseconds(200 * retry)),
                Policy.TimeoutAsync(TimeSpan.FromSeconds(2)),
                Policy.Handle<Exception>().CircuitBreakerAsync(2, TimeSpan.FromSeconds(10))
            );
        }

        public async Task<Greeting?> GetLastGreetingAsync()
        {
            return await _policy.ExecuteAsync(() => _inner.GetLastGreetingAsync());
        }

        public async Task<Greeting?> GetGreetingByIdAsync(int id)
        {
            return await _policy.ExecuteAsync(() => _inner.GetGreetingByIdAsync(id));
        }

        public async Task SaveGreetingAsync(Greeting greeting)
        {
            await _policy.ExecuteAsync(() => _inner.SaveGreetingAsync(greeting));
        }

        public async Task<IEnumerable<Greeting>> GetGreetingsAsync(int page, int pageSize, string orderBy, bool desc, string? filter)
        {
            return await _policy.ExecuteAsync(() => _inner.GetGreetingsAsync(page, pageSize, orderBy, desc, filter));
        }

        public async Task<IEnumerable<Greeting>> GetAllGreetingsAsync()
        {
            return await _policy.ExecuteAsync(() => _inner.GetAllGreetingsAsync());
        }
    }
}
