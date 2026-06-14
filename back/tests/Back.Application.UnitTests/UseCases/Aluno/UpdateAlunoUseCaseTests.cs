using Back.Application.DTOs.Aluno;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Aluno;
using Back.Domain.Entities.Aluno;
using Back.Domain.Entities.Turma;
using FluentAssertions;
using Moq;

namespace Back.Application.UnitTests.UseCases.Aluno;

public class UpdateAlunoUseCaseTests
{
    [Fact]
    public async Task Deve_Atualizar_Aluno_Corretamente()
    {
        // Arrange
        var alunoRepo = new Mock<IAlunoRepository>();
        var turmaRepo = new Mock<ITurmaRepository>();
        var identity = new Mock<IIdentityService>();

        var aluno = new AlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("Antigo")
            .WithEmail("old@ifpe.edu.br")
            .WithMatricula("111")
            .WithTurmaId(Guid.NewGuid())
            .WithIdentityUserId("UID")
            .Build();

        var novaTurma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithPeriodo("2024.1")
            .WithTurno("Manhã")
            .WithCursoId(Guid.NewGuid())
            .Build();

        alunoRepo.Setup(r => r.GetByIdAsync(aluno.Id))
                 .ReturnsAsync(aluno);

        turmaRepo.Setup(r => r.GetByIdAsync(novaTurma.Id))
                 .ReturnsAsync(novaTurma);

        identity.Setup(i => i.UpdateUserAsync("UID", "novo@ifpe.edu.br", "123"))
                .ReturnsAsync((true, Array.Empty<string>()));

        var useCase = new UpdateAlunoUseCase(alunoRepo.Object, turmaRepo.Object, identity.Object);

        // ⚠️ Aqui é onde tinha o erro: usar construtor que não existe
        var req = new UpdateAlunoRequest
        {
            Nome = "Novo",
            Email = "novo@ifpe.edu.br",
            Matricula = "222",
            TurmaId = novaTurma.Id,
            Senha = "123"
        };

        // Act
        var result = await useCase.ExecuteAsync(aluno.Id, req);

        // Assert
        result.Email.Should().Be("novo@ifpe.edu.br");
        result.Nome.Should().Be("Novo");
        aluno.TurmaId.Should().Be(novaTurma.Id);

        alunoRepo.Verify(r => r.UpdateAsync(aluno), Times.Once);
    }
}
