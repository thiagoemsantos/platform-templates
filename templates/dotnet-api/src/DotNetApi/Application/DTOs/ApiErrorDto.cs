namespace DotNetApi.Application.DTOs
{
    /// <summary>
    /// DTO para retorno de erro em APIs.
    /// </summary>
    public class ApiErrorDto
    {
        /// <example>Mensagem de erro detalhada.</example>
        public required string Error { get; set; }
    }
}