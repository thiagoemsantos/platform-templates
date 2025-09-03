# Platform Templates

Este repositório contém templates de arquitetura para APIs .NET modernas, seguindo boas práticas de Clean Architecture, SOLID, resiliência, logging estruturado, cache e testes automatizados.

## Estrutura
- **templates/dotnet-api/**: Template principal de API .NET 8, com exemplos de controllers, services, middlewares, repositórios, DTOs e helpers.
- **tests/**: Testes unitários para controllers, services, helpers e métodos de extensão. Cobrem todos os fluxos de configuração, valores nulos/inválidos e exceções.

## Cobertura de Testes
- Os testes unitários garantem cobertura máxima dos fluxos de configuração e validação.
- Branches de DI, decorators e instrumentação de middlewares são cobertos por testes de integração, recomendados para automação de QA.
- O relatório de cobertura pode ser gerado via Coverlet e ReportGenerator, disponível em `templates/dotnet-api/tests/coverage-report/index.html`.

## Recomendações para QA
- Testes de integração e regressão devem ser construídos pelo time de QA, validando a instrumentação real dos middlewares, DI e decorators.
- O projeto está pronto para automação de testes end-to-end.

## Como usar
1. Clone o repositório.
2. Navegue até a pasta `templates/dotnet-api`.
3. Execute `dotnet restore` e `dotnet build`.
4. Para rodar os testes unitários: `dotnet test templates/dotnet-api/tests/DotNetApi.Tests/DotNetApi.Tests.csproj`.
5. Para gerar o relatório de cobertura: `dotnet test ... /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=coverage.xml` e depois `reportgenerator ...`.

## Licença
MIT
