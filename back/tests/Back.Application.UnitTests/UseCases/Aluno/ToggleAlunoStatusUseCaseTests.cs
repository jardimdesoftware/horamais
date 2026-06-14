using Back.Application.UseCases.Aluno;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Aluno;
using FluentAssertions;
using Moq;

namespace Back.Application.UnitTests.UseCases.Aluno;

public class ToggleAlunoStatusUseCaseTests
{
    [Fact]
    public async Task Deve_Togglear_Status_Do_Aluno()
    {
        var repo = new Mock<IAlunoRepository>();
        var aluno = new AlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("Teste")
            .WithEmail("t@ifpe.edu.br")
            .WithMatricula("1")
            .WithTurmaId(Guid.NewGuid())
            .WithIdentityUserId("123")
            .Build();

        aluno.IsAtivo.Should().BeTrue();

        repo.Setup(r => r.GetByIdAsync(aluno.Id))
            .ReturnsAsync(aluno);

        var useCase = new ToggleAlunoStatusUseCase(repo.Object);

        await useCase.ExecuteAsync(aluno.Id);

        aluno.IsAtivo.Should().BeFalse();
        repo.Verify(r => r.UpdateAsync(aluno), Times.Once());
    }
}
