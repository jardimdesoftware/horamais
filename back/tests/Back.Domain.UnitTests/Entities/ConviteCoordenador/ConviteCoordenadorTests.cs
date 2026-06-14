using Back.Domain.Entities.Convite;
using FluentAssertions;

namespace Back.Domain.UnitTests;

public class ConviteCoordenadorTests
{
    [Fact]
    public void Convite_Valido_Deve_Passar()
    {
        var convite = new ConviteCoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("teste@ifpe.edu.br")
            .WithCursoId(Guid.NewGuid())
            .WithToken("token123")
            .WithExpiracao(DateTime.UtcNow.AddDays(1))
            .Build();

        var results = ValidationHelper.ValidateObject(convite);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Convite_Sem_Email_Deve_Falhar()
    {
        var convite = new ConviteCoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithCursoId(Guid.NewGuid())
            .WithToken("abc")
            .Build();

        var results = ValidationHelper.ValidateObject(convite);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(ConviteCoordenador.Email)));
    }

    [Fact]
    public void Convite_Com_Email_Invalido_Deve_Falhar()
    {
        var convite = new ConviteCoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("errado")
            .WithCursoId(Guid.NewGuid())
            .WithToken("abc")
            .Build();

        var results = ValidationHelper.ValidateObject(convite);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(ConviteCoordenador.Email)));
    }
}
