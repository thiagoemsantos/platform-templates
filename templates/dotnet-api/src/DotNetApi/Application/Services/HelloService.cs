using DotNetApi.Domain.Entities;
using DotNetApi.Domain.Interfaces;
using DotNetApi.Application.DTOs;

namespace DotNetApi.Application.Services
{
    /// <summary>
    /// Serviço de aplicação responsável pela lógica de negócio relacionada à saudação.
    /// </summary>
    public class HelloService
    {
        private readonly IGreetingRepository _repo;
        private readonly ILogger<HelloService> _logger;

        public HelloService(IGreetingRepository repo, ILogger<HelloService> logger)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Retorna a saudação mais recente ou null se não existir.
        /// </summary>
        public async Task<Greeting?> GetLastGreetingAsync()
        {
            try
            {
                var result = await _repo.GetLastGreetingAsync();
                _logger.LogInformation("Saudação mais recente recuperada: {@Greeting}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar saudação mais recente.");
                throw;
            }
        }

        /// <summary>
        /// Retorna a saudação pelo Id ou null se não existir.
        /// </summary>
        /// <param name="id">Id da saudação.</param>
        /// <exception cref="ArgumentException">Se o Id for inválido.</exception>
        public async Task<Greeting?> GetGreetingByIdAsync(int id)
        {
            ValidateId(id);
            try
            {
                var result = await _repo.GetGreetingByIdAsync(id);
                _logger.LogInformation("Saudação recuperada por id {Id}: {@Greeting}", id, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar saudação por id {Id}.", id);
                throw;
            }
        }

        /// <summary>
        /// Orquestra a persistência de uma nova saudação, aplicando todas as validações e regras de negócio.
        /// </summary>
        /// <param name="greeting">Entidade Greeting a ser salva.</param>
        /// <exception cref="ArgumentException">Se a mensagem for nula, vazia ou inválida.</exception>
        public async Task SaveGreetingAsync(Greeting greeting)
        {
            ValidateGreetingNotNull(greeting);
            ValidateMessageNotEmpty(greeting.Message);
            ValidateMessageLength(greeting.Message);
            try
            {
                await _repo.SaveGreetingAsync(greeting);
                ValidateId(greeting.Id); // Valida Id após atribuição pelo repositório
                _logger.LogInformation("Saudação salva: {@Greeting}", greeting);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar saudação: {@Greeting}", greeting);
                throw;
            }
        }

        /// <summary>
        /// Retorna saudações paginadas, ordenadas e filtradas.
        /// </summary>
    public async Task<PagedGreetingDto> GetPagedGreetingsAsync(int page, int pageSize, string orderBy, bool desc, string? filter)
        {
            try
            {
                var greetings = await _repo.GetGreetingsAsync(page, pageSize, orderBy, desc, filter);
                _logger.LogInformation("Saudações paginadas recuperadas. Total: {Count}", greetings?.Count() ?? 0);

                var items = (greetings ?? Enumerable.Empty<Greeting>()).Select(g => new GreetingDto
                {
                    Id = g.Id,
                    Message = g.Message,
                    CreatedAt = g.CreatedAt
                }).ToList();

                // Total de itens considerando filtro
                int totalItems = items.Count;

                var links = GreetingLinksAssembler.BuildLinks(page, pageSize, totalItems);

                return new PagedGreetingDto
                {
                    Items = items,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    Links = links
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar saudações paginadas.");
                throw;
            }
        }

        /// <summary>
        /// Retorna todas as saudações cadastradas.
        /// </summary>
        public async Task<IEnumerable<Greeting>> GetAllGreetingsAsync()
        {
            try
            {
                var result = await _repo.GetAllGreetingsAsync();
                _logger.LogInformation("Todas as saudações recuperadas. Total: {Count}", result?.Count() ?? 0);
                return result!;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recuperar todas as saudações.");
                throw;
            }
        }

        /// <summary>
        /// Regra de negócio: Id deve ser maior que zero.
        /// </summary>
        private void ValidateId(int id)
        {
            if (id <= 0)
                throw new ArgumentException("O Id deve ser maior que zero.");
        }

        /// <summary>
        /// Regra de negócio: Greeting não pode ser nulo.
        /// </summary>
        private void ValidateGreetingNotNull(Greeting? greeting)
        {
            if (greeting == null)
                throw new ArgumentException("A saudação não pode ser nula.");
        }

        /// <summary>
        /// Regra de negócio: Mensagem não pode ser nula ou vazia.
        /// </summary>
        private void ValidateMessageNotEmpty(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("A mensagem da saudação não pode ser nula ou vazia.");
        }

        /// <summary>
        /// Regra de negócio: Mensagem não pode exceder 200 caracteres.
        /// </summary>
        private void ValidateMessageLength(string message)
        {
            if (message.Length > 200)
                throw new ArgumentException("A mensagem da saudação não pode ter mais de 200 caracteres.");
        }
    }
}