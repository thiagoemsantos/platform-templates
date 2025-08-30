# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/DotNetApi/DotNetApi.csproj", "src/DotNetApi/"]
RUN dotnet restore "src/DotNetApi/DotNetApi.csproj"

COPY . .
WORKDIR /src/src/DotNetApi
RUN dotnet publish DotNetApi.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DotNetApi.dll"]