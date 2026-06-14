using System.Net;

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
    }
}
