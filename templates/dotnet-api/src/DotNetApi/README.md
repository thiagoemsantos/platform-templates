# 🛠️ Template: API .NET Moderna

PoC de template para APIs .NET com boas práticas modernas, ideal para:

- Migração de Web Apps para Kubernetes
- Observabilidade nativa
- Segurança com Azure Key Vault
- Onboarding rápido de times

---

## ✅ Recursos Incluídos

| Recurso               | Tecnologia                     |
|-----------------------|--------------------------------|
| API Web               | ASP.NET Core Minimal API       |
| Documentação          | Swagger / OpenAPI              |
| Health Checks         | Built-in .NET Health Checks    |
| Observabilidade       | OpenTelemetry (traces/metrics) |
| Segurança de Secrets  | Azure Key Vault + MSI          |
| Containerização       | Docker multi-stage             |
| Deploy Kubernetes     | Pronto para AKS                |

---

## 🚀 Como Usar

### 1. Localmente

```bash
dotnet run --project src/DotNetApi
```

# DotNetApi Platform Template

Este projeto é um template de API moderna em .NET, estruturado segundo os princípios de **Clean Architecture** e **SOLID**.  
A arquitetura facilita extensibilidade, testes, manutenção e integração com múltiplos bancos de dados.

## Estrutura de Pastas

```
src/
  DotNetApi/
    Domain/
      Entities/
      Interfaces/
    Application/
      DTOs/
      Services/
    Infrastructure/
      Repositories/
    Middlewares/
    Controllers/
    Program.cs
    appsettings.json
```

## Principais Recursos

- **Clean Architecture**: Separação clara entre domínio, aplicação, infraestrutura e apresentação.
- **SOLID**: Classes e interfaces com responsabilidades únicas e bem definidas.
- **Persistência configurável**: Suporte a múltiplos bancos (SQLite, MongoDB, etc.) via `appsettings.json`.
- **DTOs**: Transferência de dados desacoplada das entidades de domínio.
- **Middleware de CorrelationId**: Rastreabilidade e enriquecimento dos logs.
- **Swagger**: Documentação automática dos endpoints (habilitado em ambiente de desenvolvimento).
- **Health Checks**: Endpoint para monitoramento de saúde da aplicação.

## Como configurar a persistência

Edite o arquivo `appsettings.json` para definir os bancos de dados utilizados:

```json
"Persistence": {
  "Provider": "Sqlite",
  "MultiProvider": [ "Sqlite", "MongoDB" ],
  "MongoDbName": "MyDb"
},
"ConnectionStrings": {
  "Sqlite": "Data Source=local.db",
  "MongoDB": "mongodb://localhost:27017"
}
```

## Executando o projeto

1. Instale os pacotes NuGet necessários:
   ```sh
   dotnet restore
   ```
2. Execute a aplicação:
   ```sh
   dotnet run --project src/DotNetApi/DotNetApiTemplate.csproj
   ```
3. Acesse os endpoints:
   - `GET /api/hello` — Retorna a saudação mais recente.
   - `POST /api/hello` — Cria uma nova saudação.
   - `GET /health` — Verifica a saúde da aplicação.
   - `GET /swagger` — Documentação interativa (apenas em ambiente de desenvolvimento).

## Testes

Organize seus testes em uma pasta separada, por exemplo:
```
tests/
  DotNetApi.Tests/
```

## Contribuição

Siga o padrão de organização das camadas e mantenha os princípios de Clean Architecture e SOLID.

---

**Dúvidas ou sugestões?**  
Abra uma issue ou contribua com um pull
