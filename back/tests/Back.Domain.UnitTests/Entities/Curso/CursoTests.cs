using Back.Domain.Entities.Curso;
using FluentAssertions;

namespace Back.Domain.UnitTests;

public class CursoTests
{
    [Fact]
    public void Curso_Valido_Deve_Passar()
    {
        var curso = new CursoBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("Engenharia")
            .Build();

        var results = ValidationHelper.ValidateObject(curso);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Curso_Sem_Nome_Deve_Falhar()
    {
        var curso = new CursoBuilder()
            .WithId(Guid.NewGuid())
            .Build();

        var results = ValidationHelper.ValidateObject(curso);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(Curso.Nome)));
    }

    [Fact]
    public void Curso_Deve_Inicializar_Listas()
    {
        var curso = new CursoBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("Direito")
            .Build();

        curso.Turmas.Should().NotBeNull();
        curso.Coordenadores.Should().NotBeNull();
    }
}
