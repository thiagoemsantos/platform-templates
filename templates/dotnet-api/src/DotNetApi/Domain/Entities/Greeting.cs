namespace DotNetApi.Domain.Entities
{
    /// <summary>
    /// Entidade de saudação.
    /// </summary>
    public class Greeting
    {
        /// <summary>
        /// Identificador único da saudação.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Mensagem da saudação.
        /// </summary>
        public required string Message { get; set; }

        /// <summary>
        /// Data e hora de criação da saudação.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}