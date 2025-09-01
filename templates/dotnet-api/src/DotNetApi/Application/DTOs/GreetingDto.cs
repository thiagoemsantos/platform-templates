namespace DotNetApi.Application.DTOs
{
    /// <summary>
    /// DTO para retorno de saudação.
    /// </summary>
    public class GreetingDto
    {
        public int Id { get; set; }

        /// <example>Hello World!</example>
        public required string Message { get; set; }

        /// <example>2025-08-30T21:00:00Z</example>
        public DateTime CreatedAt { get; set; }
    }
}