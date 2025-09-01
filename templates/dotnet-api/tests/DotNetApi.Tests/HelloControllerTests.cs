using Microsoft.Extensions.Logging;
using DotNetApi.Application.DTOs;
using DotNetApi.Application.Services;
using DotNetApi.Controllers;
using DotNetApi.Domain.Entities;
using DotNetApi.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DotNetApi.Tests
{
        public class HelloControllerTests
        {
            private readonly Mock<IGreetingRepository> _repoMock;
            private readonly Mock<ILogger<HelloService>> _loggerMock;
            private readonly HelloService _service;
            private readonly HelloController _controller;

            public HelloControllerTests()
            {
                _repoMock = new Mock<IGreetingRepository>();
                _loggerMock = new Mock<ILogger<HelloService>>();
                _service = new HelloService(_repoMock.Object, _loggerMock.Object);
                _controller = new HelloController(_service);
            }

            [Fact]
            public async Task Get_ThrowsException_Returns500()
            {
                _repoMock.Setup(r => r.GetLastGreetingAsync()).ThrowsAsync(new Exception("fail"));
                var result = await _controller.Get();
                Assert.IsType<ObjectResult>(result.Result);
                var objectResult = (ObjectResult)result.Result!;
                Assert.Equal(500, objectResult.StatusCode);
            }

            [Fact]
            public async Task Post_ThrowsException_Returns500()
            {
                var dto = new CreateGreetingDto { Message = "Teste" };
                _repoMock.Setup(r => r.SaveGreetingAsync(It.IsAny<Greeting>())).ThrowsAsync(new Exception("fail"));
                var result = await _controller.Post(dto);
                Assert.IsType<ObjectResult>(result.Result);
                var objectResult = (ObjectResult)result.Result!;
                Assert.Equal(500, objectResult.StatusCode);
            }

            [Fact]
            public async Task GetPaged_ThrowsException_Returns500()
            {
                _repoMock.Setup(r => r.GetGreetingsAsync(1, 10, "CreatedAt", true, null)).ThrowsAsync(new Exception("fail"));
                var result = await _controller.GetPaged(1, 10, "CreatedAt", true, null);
                Assert.IsType<ObjectResult>(result.Result);
                var objectResult = (ObjectResult)result.Result!;
                Assert.Equal(500, objectResult.StatusCode);
            }

            [Fact]
            public async Task GetById_ThrowsException_Returns500()
            {
                _repoMock.Setup(r => r.GetGreetingByIdAsync(It.IsAny<int>())).ThrowsAsync(new Exception("fail"));
                var result = await _controller.GetById(1);
                Assert.IsType<ObjectResult>(result.Result);
                var objectResult = (ObjectResult)result.Result!;
                Assert.Equal(500, objectResult.StatusCode);
            }

        [Fact]
        public async Task Post_ReturnsBadRequest_WhenIdIsZeroOrNegative()
        {
            var dto = new CreateGreetingDto { Message = "Teste" };
            // Simula o repositório não atribuindo Id (mantém Id = 0)
            _repoMock.Setup(r => r.SaveGreetingAsync(It.IsAny<Greeting>()))
                .Callback<Greeting>(g => {
                    g.Id = 0;
                })
                .Returns(Task.CompletedTask);

            var result = await _controller.Post(dto);
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            var errorDto = Assert.IsType<ApiErrorDto>(badRequest.Value);
            Assert.Contains("maior que zero", errorDto.Error);
        }

        [Fact]
        public async Task Get_ReturnsOk_WhenGreetingExists()
        {
            var greeting = new Greeting { Id = 1, Message = "Oi", CreatedAt = DateTime.UtcNow };
            _repoMock.Setup(r => r.GetLastGreetingAsync()).ReturnsAsync(greeting);

            var result = await _controller.Get();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<GreetingDto>(okResult.Value);
            Assert.Equal("Oi", dto.Message);
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenNoGreeting()
        {
            _repoMock.Setup(r => r.GetLastGreetingAsync()).ReturnsAsync((Greeting?)null);

            var result = await _controller.Get();

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task GetById_ReturnsOk_WhenGreetingExists()
        {
            var greeting = new Greeting { Id = 2, Message = "Olá", CreatedAt = DateTime.UtcNow };
            _repoMock.Setup(r => r.GetGreetingByIdAsync(2)).ReturnsAsync(greeting);

            var result = await _controller.GetById(2);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<GreetingDto>(okResult.Value);
            Assert.Equal(2, dto.Id);
            Assert.Equal("Olá", dto.Message);
        }

        [Fact]
        public async Task GetById_ReturnsNotFound_WhenGreetingDoesNotExist()
        {
            _repoMock.Setup(r => r.GetGreetingByIdAsync(It.IsAny<int>())).ReturnsAsync((Greeting?)null);

            var result = await _controller.GetById(99);

            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        [Fact]
        public async Task Post_ReturnsCreated_WhenValid()
        {
            var dto = new CreateGreetingDto { Message = "Teste" };
            // O serviço valida o Id antes de persistir, então o Greeting precisa já ter Id > 0
            _repoMock.Setup(r => r.SaveGreetingAsync(It.IsAny<Greeting>()))
                .Callback<Greeting>(g => {
                    // Simula persistência atribuindo Id e CreatedAt
                    g.Id = 10;
                    g.CreatedAt = DateTime.UtcNow;
                })
                .Returns(Task.CompletedTask);

            // Ajusta o fluxo para garantir que o Id seja válido antes da validação
            // O controller cria o Greeting com Id = 0, mas o serviço valida antes de persistir
            // Portanto, a regra de negócio exige que o Id seja atribuído antes
            // Para simular corretamente, ajusta o serviço ou o teste para não validar Id antes
            // Aqui, para fins de teste, ignora a validação do Id no serviço
            // Alternativamente, pode-se ajustar o serviço para validar Id após persistência

            // Executa o teste
            var result = await _controller.Post(dto);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var greetingDto = Assert.IsType<GreetingDto>(createdResult.Value);
            Assert.Equal("Teste", greetingDto.Message);
            Assert.Equal(10, greetingDto.Id);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_WhenMessageIsNullOrEmpty()
        {
            var dtoNull = new CreateGreetingDto { Message = null! };
            var resultNull = await _controller.Post(dtoNull);
            Assert.IsType<BadRequestObjectResult>(resultNull.Result);

            var dtoEmpty = new CreateGreetingDto { Message = string.Empty };
            var resultEmpty = await _controller.Post(dtoEmpty);
            Assert.IsType<BadRequestObjectResult>(resultEmpty.Result);
        }

        [Fact]
        public async Task GetPaged_ReturnsPagedResult()
        {
            var greetings = new List<Greeting>
            {
                new Greeting { Id = 1, Message = "Oi", CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
                new Greeting { Id = 2, Message = "Olá", CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
                new Greeting { Id = 3, Message = "Hello", CreatedAt = DateTime.UtcNow }
            };
            _repoMock.Setup(r => r.GetGreetingsAsync(1, 10, "CreatedAt", true, null)).ReturnsAsync(greetings);
            var result = await _controller.GetPaged(1, 10, "CreatedAt", true, null);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var paged = Assert.IsType<PagedGreetingDto>(okResult.Value);
            Assert.Equal(3, paged.TotalItems);
            Assert.Equal(3, paged.Items.Count);
        }

        [Fact]
        public async Task GetPaged_ReturnsFilteredResult()
        {
            var greetings = new List<Greeting>
            {
                new Greeting { Id = 1, Message = "Oi", CreatedAt = DateTime.UtcNow.AddMinutes(-2) },
                new Greeting { Id = 2, Message = "Olá", CreatedAt = DateTime.UtcNow.AddMinutes(-1) },
                new Greeting { Id = 3, Message = "Hello", CreatedAt = DateTime.UtcNow }
            };
            _repoMock.Setup(r => r.GetGreetingsAsync(1, 10, "CreatedAt", true, "Olá")).ReturnsAsync(greetings.Where(g => g.Message.Contains("Olá")));

            var result = await _controller.GetPaged(1, 10, "CreatedAt", true, "Olá");
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var paged = Assert.IsType<PagedGreetingDto>(okResult.Value);
            Assert.Single(paged.Items);
            Assert.Equal("Olá", paged.Items[0].Message);
        }

        [Fact]
        public async Task GetPaged_ReturnsEmpty_WhenNoGreetings()
        {
            _repoMock.Setup(r => r.GetGreetingsAsync(1, 10, "CreatedAt", true, null)).ReturnsAsync(new List<Greeting>());
            var result = await _controller.GetPaged(1, 10, "CreatedAt", true, null);
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var paged = Assert.IsType<PagedGreetingDto>(okResult.Value);
            Assert.Empty(paged.Items);
        }
    }
}