using System.Net;
using System.Text.Json;

using FluentAssertions;

namespace Back.API.IntegrationTests;

[Collection("Integration")]
public class SmokeTests
{
    private readonly CustomWebApplicationFactory _factory;

    public SmokeTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Aplicacao_Sobe_E_ExpoeSwagger()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Horas Discentes API");
        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;
        var bearer = root.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer");
        bearer.GetProperty("type").GetString().Should().Be("http");
        bearer.GetProperty("scheme").GetString().Should().Be("bearer");
        root.GetProperty("security")[0].GetProperty("Bearer").GetArrayLength().Should().Be(0);
    }
}
