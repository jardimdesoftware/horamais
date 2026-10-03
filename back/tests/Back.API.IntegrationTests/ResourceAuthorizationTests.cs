using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Back.Application.DTOs.Auth;
using Back.Domain.Entities.Curso;
using Back.Infrastructure.Persistence.Context;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Back.API.IntegrationTests;

[Collection("Integration")]
public class ResourceAuthorizationTests(CustomWebApplicationFactory factory)
{
    [Fact]
    public async Task Coordenador_Nao_Pode_Listar_Recursos_De_Outro_Curso()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var campusId = db.Campi.Select(c => c.Id).First();
        var otherCourseId = Guid.NewGuid();
        db.Cursos.Add(new CursoBuilder().WithId(otherCourseId)
            .WithNome("Outro curso de teste").WithCampusId(campusId).Build());
        await db.SaveChangesAsync();

        var client = factory.CreateClient();
        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "coordenador.teste@ifpe.edu.br",
            senha = "Coord@2026"
        });
        login.EnsureSuccessStatusCode();
        var user = await login.Content.ReadFromJsonAsync<LoginResponseDto>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", user!.Token);

        (await client.GetAsync($"/api/turma/curso/{otherCourseId}"))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.GetAsync($"/api/certificado/por-curso/{otherCourseId}"))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
