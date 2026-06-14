using System.Net;

using FluentAssertions;

namespace Back.API.IntegrationTests;

[Collection("Integration")]
public class CursoEndpointsTests
{
    private readonly CustomWebApplicationFactory _factory;

    public CursoEndpointsTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task ListarCursos_SemToken_RetornaUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/curso");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ListarCursos_ComTokenAdmin_RetornaCursosSemeados()
    {
        var client = await _factory.CreateAdminClientAsync();

        var response = await client.GetAsync("/api/curso");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain(CustomWebApplicationFactory.CursoNome);
    }
}
