namespace DotNetApi.Application.DTOs
{
    /// <summary>
    /// DTO para criação de saudação.
    /// </summary>
    public class CreateGreetingDto
    {
        public required string Message { get; set; }
    }
}