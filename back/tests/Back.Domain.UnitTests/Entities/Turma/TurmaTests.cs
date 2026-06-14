using Back.Domain.Entities.Turma;
using FluentAssertions;

namespace Back.Domain.UnitTests;

public class TurmaTests
{
    [Fact]
    public void Turma_Valida_Deve_Passar()
    {
        var turma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithPeriodo("2024.1")
            .WithTurno("Noite")
            .WithCursoId(Guid.NewGuid())
            .WithPossuiExtensao(true)
            .Build();

        var results = ValidationHelper.ValidateObject(turma);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Turma_Sem_Periodo_Deve_Falhar()
    {
        var turma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithTurno("Noite")
            .WithCursoId(Guid.NewGuid())
            .Build();

        var results = ValidationHelper.ValidateObject(turma);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(Turma.Periodo)));
    }

    [Fact]
    public void Turma_Sem_Turno_Deve_Falhar()
    {
        var turma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithPeriodo("2024.1")
            .WithCursoId(Guid.NewGuid())
            .Build();

        var results = ValidationHelper.ValidateObject(turma);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(Turma.Turno)));
    }

    [Fact]
    public void Turma_Deve_Inicializar_Lista_Alunos()
    {
        var turma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithPeriodo("2024.1")
            .WithTurno("Noite")
            .WithCursoId(Guid.NewGuid())
            .Build();

        turma.Alunos.Should().NotBeNull();
        turma.Alunos.Should().BeEmpty();
    }
}
