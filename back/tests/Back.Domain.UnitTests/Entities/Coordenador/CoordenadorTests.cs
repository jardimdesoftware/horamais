using Back.Domain.Entities.Coordenador;
using FluentAssertions;

namespace Back.Domain.UnitTests;

public class CoordenadorTests
{
    [Fact]
    public void Coordenador_Valido_Deve_Passar()
    {
        var coordenador = new CoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("Fulano da Silva")
            .WithNumeroPortaria("12345")
            .WithDOU("DOU123")
            .WithEmail("teste@ifpe.edu.br")
            .WithCursoId(Guid.NewGuid())
            .WithIdentityUserId("user-123")
            .Build();

        var results = ValidationHelper.ValidateObject(coordenador);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Coordenador_Sem_Nome_Deve_Falhar()
    {
        var coordenador = new CoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithNumeroPortaria("123")
            .WithDOU("dou")
            .WithEmail("teste@ifpe.edu.br")
            .WithCursoId(Guid.NewGuid())
            .WithIdentityUserId("user-1")
            .Build();

        var results = ValidationHelper.ValidateObject(coordenador);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(Coordenador.Nome)));
    }

    [Fact]
    public void Coordenador_Com_Email_Invalido_Deve_Falhar()
    {
        var coordenador = new CoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("Fulano")
            .WithNumeroPortaria("111")
            .WithDOU("dou")
            .WithEmail("emailruim")
            .WithCursoId(Guid.NewGuid())
            .WithIdentityUserId("user-1")
            .Build();

        var results = ValidationHelper.ValidateObject(coordenador);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(Coordenador.Email)));
    }
}
