using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Aluno;
using FluentAssertions;
using Moq;
using Back.Domain.Entities.Aluno;

namespace Back.Application.UnitTests.UseCases.Aluno;

public class GetAlunoByIdUseCaseTests
{
    private readonly Mock<IAlunoRepository> _repo = new();

    [Fact]
    public async Task Deve_Retornar_Aluno_Quando_Encontrado()
    {
        var alunoId = Guid.NewGuid();

        var aluno = new AlunoBuilder()
            .WithId(alunoId)
            .WithNome("João")
            .WithEmail("joao@ifpe.edu.br")
            .WithMatricula("2023001")
            .WithTurmaId(Guid.NewGuid())
            .WithIdentityUserId("uid")
            .Build();

        _repo.Setup(r => r.GetByIdAsync(alunoId))
            .ReturnsAsync(aluno);

        var useCase = new GetAlunoByIdUseCase(_repo.Object);

        var result = await useCase.ExecuteAsync(alunoId);

        result.Should().NotBeNull();
        result.Id.Should().Be(alunoId);
        result.Email.Should().Be("joao@ifpe.edu.br");
    }

    [Fact]
    public async Task Deve_Lancar_Excecao_Quando_Aluno_Nao_Existir()
    {
        var useCase = new GetAlunoByIdUseCase(_repo.Object);

        Func<Task> act = () => useCase.ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
