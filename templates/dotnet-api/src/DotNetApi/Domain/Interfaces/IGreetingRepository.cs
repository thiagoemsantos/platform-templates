using DotNetApi.Domain.Entities;

namespace DotNetApi.Domain.Interfaces
{
    /// <summary>
    /// Contrato para repositórios de persistência da entidade Greeting.
    /// </summary>
    public interface IGreetingRepository
    {
    /// <summary>
    /// Recupera a saudação mais recente.
    /// </summary>
    Task<Greeting?> GetLastGreetingAsync();

    /// <summary>
    /// Recupera saudações paginadas, ordenadas e filtradas.
    /// </summary>
    /// <param name="page">Página (1-based)</param>
    /// <param name="pageSize">Itens por página</param>
    /// <param name="orderBy">Campo para ordenação</param>
    /// <param name="desc">Ordenação descendente?</param>
    /// <param name="filter">Filtro por mensagem</param>
    /// <returns>Lista paginada de saudações</returns>
    Task<IEnumerable<Greeting>> GetGreetingsAsync(int page, int pageSize, string orderBy, bool desc, string? filter);

    /// <summary>
    /// Recupera uma saudação pelo seu identificador.
    /// </summary>
    /// <param name="id">O identificador da saudação.</param>
    /// <returns>A saudação correspondente ao identificador.</returns>
    Task<Greeting?> GetGreetingByIdAsync(int id);

    /// <summary>
    /// Persiste uma nova saudação.
    /// </summary>
    /// <param name="greeting">A saudação a ser persistida.</param>
    Task SaveGreetingAsync(Greeting greeting);

    /// <summary>
    /// Recupera todas as saudações cadastradas.
    /// </summary>
    /// <returns>Lista de todas as saudações.</returns>
    Task<IEnumerable<Greeting>> GetAllGreetingsAsync();
    }
}