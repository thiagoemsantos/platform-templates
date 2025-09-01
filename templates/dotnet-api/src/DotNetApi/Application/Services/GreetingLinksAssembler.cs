using DotNetApi.Application.DTOs;

namespace DotNetApi.Application.Services
{
    public static class GreetingLinksAssembler
    {
        /// <summary>
        /// Monta os links HATEOAS para paginação de saudações.
        /// </summary>
        /// <param name="page">Página atual</param>
        /// <param name="pageSize">Itens por página</param>
        /// <param name="totalItems">Total de itens</param>
        /// <returns>Lista de links HATEOAS</returns>
        public static List<LinkDto> BuildLinks(int page, int pageSize, int totalItems)
        {
            var links = new List<LinkDto>
            {
                new LinkDto { Rel = "self", Href = $"/api/v1/hello/list?page={page}&pageSize={pageSize}" },
                new LinkDto { Rel = "create", Href = "/api/v1/hello" }
            };

            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            if (page > 1 && totalPages > 0)
                links.Add(new LinkDto { Rel = "prev", Href = $"/api/v1/hello/list?page={page-1}&pageSize={pageSize}" });

            if (page < totalPages)
                links.Add(new LinkDto { Rel = "next", Href = $"/api/v1/hello/list?page={page+1}&pageSize={pageSize}" });

            return links;
        }
    }
}
