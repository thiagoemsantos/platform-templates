using DotNetApi.Application.Services;
using DotNetApi.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace DotNetApi.Controllers
{
    /// <summary>
    /// Controller responsável pelos endpoints de saudação.
    /// </summary>
    [ApiController]
    [Route("api/v1/hello")]
    public class HelloController : ControllerBase
    {
        private readonly HelloService _helloService;

        public HelloController(HelloService helloService)
        {
            _helloService = helloService;
        }

    /// <summary>
    /// Lista saudações com paginação, ordenação e filtro.
    /// </summary>
    /// <remarks>
    /// <b>Exemplo de requisição:</b>
    /// <code>
    /// GET /api/v1/hello/list?page=1&amp;pageSize=10&amp;orderBy=CreatedAt&amp;desc=true&amp;filter=Olá
    /// </code>
    /// </remarks>
    /// <param name="page">Número da página (default: 1)</param>
    /// <param name="pageSize">Itens por página (default: 10)</param>
    /// <param name="orderBy">Campo para ordenação: CreatedAt ou Message (default: CreatedAt)</param>
    /// <param name="desc">Ordenação descendente? (default: true)</param>
    /// <param name="filter">Filtro por mensagem (opcional)</param>
    /// <returns>Lista paginada de GreetingDto com metadados e links HATEOAS.</returns>
            [HttpGet("list")]
            [ProducesResponseType(typeof(PagedGreetingDto), 200)]
            public async Task<ActionResult<PagedGreetingDto>> GetPaged(
                int page = 1,
                int pageSize = 10,
                string orderBy = "CreatedAt",
                bool desc = true,
                string? filter = null)
            {
                try
                {
                    var result = await _helloService.GetPagedGreetingsAsync(page, pageSize, orderBy, desc, filter);
                    return Ok(result);
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new ApiErrorDto { Error = ex.Message });
                }
            }
        /// <summary>
        /// Recupera a saudação mais recente.
        /// </summary>
        /// <returns>GreetingDto com mensagem e data.</returns>
        [HttpGet]
        [ProducesResponseType(typeof(GreetingDto), 200)]
        [ProducesResponseType(typeof(ApiErrorDto), 404)]
        public async Task<ActionResult<GreetingDto>> Get()
        {
            try
            {
                var greeting = await _helloService.GetLastGreetingAsync();
                if (greeting == null)
                    return NotFound(new ApiErrorDto { Error = "Greeting não encontrado." });

                return Ok(new GreetingDto
                {
                    Message = greeting.Message,
                    CreatedAt = greeting.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorDto { Error = ex.Message });
            }
        }

        /// <summary>
        /// Recupera uma saudação pelo ID.
        /// </summary>
        /// <param name="id">ID da saudação.</param>
        /// <returns>GreetingDto com mensagem e data.</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(GreetingDto), 200)]
        [ProducesResponseType(typeof(ApiErrorDto), 404)]
        public async Task<ActionResult<GreetingDto>> GetById(int id)
        {
            try
            {
                var greeting = await _helloService.GetGreetingByIdAsync(id);
                if (greeting == null)
                    return NotFound(new ApiErrorDto { Error = "Greeting não encontrado." });

                return Ok(new GreetingDto
                {
                    Id = greeting.Id,
                    Message = greeting.Message,
                    CreatedAt = greeting.CreatedAt
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorDto { Error = ex.Message });
            }
        }

        /// <summary>
        /// Cria uma nova saudação.
        /// </summary>
        /// <param name="dto">DTO contendo a mensagem.</param>
        /// <returns>GreetingDto criado.</returns>
        [HttpPost]
        [ProducesResponseType(typeof(GreetingDto), 201)]
        [ProducesResponseType(typeof(ApiErrorDto), 400)]
        public async Task<ActionResult<GreetingDto>> Post([FromBody] CreateGreetingDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest(new ApiErrorDto { Error = "A mensagem não pode ser nula ou vazia." });

            var greeting = new Domain.Entities.Greeting { Id = 0, Message = dto.Message };
            try
            {
                await _helloService.SaveGreetingAsync(greeting);
                var result = new GreetingDto
                {
                    Id = greeting.Id,
                    Message = greeting.Message,
                    CreatedAt = greeting.CreatedAt
                };
                return CreatedAtAction(nameof(GetById), new { id = greeting.Id }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiErrorDto { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiErrorDto { Error = ex.Message });
            }
        }
    }
}