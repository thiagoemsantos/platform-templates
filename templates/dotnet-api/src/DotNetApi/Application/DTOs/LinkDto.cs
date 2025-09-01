namespace DotNetApi.Application.DTOs
{
    /// <summary>
    /// DTO para links HATEOAS.
    /// </summary>
    public class LinkDto
    {
        public string Rel { get; set; } = string.Empty;
        public string Href { get; set; } = string.Empty;
    }
}
