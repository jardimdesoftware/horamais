using System.Net;
using System.Net.Http.Json;

using Back.Application.DTOs.Auth;

using FluentAssertions;

namespace Back.API.IntegrationTests;

[Collection("Integration")]
public class AuthEndpointsTests
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthEndpointsTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_ComCredenciaisValidas_RetornaTokenComPerfilAdmin()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = CustomWebApplicationFactory.AdminEmail, senha = CustomWebApplicationFactory.AdminPassword });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        body.Should().NotBeNull();
        body!.Token.Should().NotBeNullOrWhiteSpace();
        body.Email.Should().Be(CustomWebApplicationFactory.AdminEmail);
        body.Role.Should().Be("ADMIN");
    }

    [Fact]
    public async Task Login_ComSenhaIncorreta_RetornaBadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = CustomWebApplicationFactory.AdminEmail, senha = "SenhaErrada@1" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ComUsuarioInexistente_RetornaBadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { email = "naoexiste@ifpe.edu.br", senha = "QualquerCoisa@1" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GoogleLogin_ComIdTokenInvalido_RetornaBadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/google-login",
            new { idToken = "token-invalido-de-teste" });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GoogleRegister_SemTicketValido_RecusaCadastro()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync(
            "/api/auth/google-register",
            new { registrationTicket = "invalid", nome = "Aluno", matricula = "20231ewbj2157", turmaCodigo = "ADS2B7" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
