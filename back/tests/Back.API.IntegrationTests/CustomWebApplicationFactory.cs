using System.Net.Http.Headers;
using System.Net.Http.Json;

using Back.Application.DTOs.Auth;

using Microsoft.AspNetCore.Mvc.Testing;

using Testcontainers.PostgreSql;

namespace Back.API.IntegrationTests;

/// <summary>
/// Sobe a API completa em memória (pipeline real: controllers, autenticação JWT,
/// EF Core/Npgsql, migrations e seeds) contra um PostgreSQL real e descartável
/// provisionado via Testcontainers. Cada execução parte de um banco limpo.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string AdminEmail = "admin.teste@ifpe.edu.br";
    public const string AdminPassword = "Admin@1234";
    public const string CursoNome = "Engenharia de Software";

    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:15")
        .WithDatabase("horas_test")
        .WithUsername("test")
        .WithPassword("test")
        .Build();

    public async Task InitializeAsync()
    {
        await _db.StartAsync();

        // O Program lê estas variáveis via AddEnvironmentVariables() ANTES de
        // construir o host, então precisam existir no processo antes da criação
        // do primeiro HttpClient (que é quando o host de teste é construído).
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", _db.GetConnectionString());
        Environment.SetEnvironmentVariable("Jwt__Key", "ChaveDeTesteIntegracao_HoraMais_2026_0123456789");
        Environment.SetEnvironmentVariable("Jwt__Issuer", "HoraMais");
        Environment.SetEnvironmentVariable("Jwt__Audience", "HoraMaisUsuarios");
        Environment.SetEnvironmentVariable("CORS_ALLOWED_ORIGIN", "http://localhost:3000");

        // Admin: criado pelo AdminSeeder no boot.
        Environment.SetEnvironmentVariable("ADMIN_EMAIL", AdminEmail);
        Environment.SetEnvironmentVariable("ADMIN_PASSWORD", AdminPassword);

        // Coordenador/curso: o CoordenadorSeeder cria o curso usado nos testes.
        Environment.SetEnvironmentVariable("COORD_EMAIL", "coordenador.teste@ifpe.edu.br");
        Environment.SetEnvironmentVariable("COORD_PASSWORD", "Coord@2026");
        Environment.SetEnvironmentVariable("COORD_NOME", "Coordenador de Teste");
        Environment.SetEnvironmentVariable("COORD_PORTARIA", "001/2026");
        Environment.SetEnvironmentVariable("COORD_DOU", "2026-01-10");
        Environment.SetEnvironmentVariable("CURSO_NOME", CursoNome);
    }

    /// <summary>
    /// Autentica como o admin semeado e devolve um HttpClient com o Bearer token configurado.
    /// </summary>
    public async Task<HttpClient> CreateAdminClientAsync()
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = AdminEmail, senha = AdminPassword });
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", body!.Token);
        return client;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _db.DisposeAsync();
        await base.DisposeAsync();
    }
}

[CollectionDefinition("Integration")]
public class IntegrationCollection : ICollectionFixture<CustomWebApplicationFactory>
{
    // Marcador: compartilha uma única instância da factory (e do container) entre
    // todas as classes de teste da coleção.
}
