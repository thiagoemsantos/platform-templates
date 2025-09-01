namespace DotNetApi.Application.DTOs
{
    /// <summary>
    /// DTO para resposta paginada de saudações.
    /// </summary>
    public class PagedGreetingDto
    {
        public List<GreetingDto> Items { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public List<LinkDto> Links { get; set; } = new();
    }
}
