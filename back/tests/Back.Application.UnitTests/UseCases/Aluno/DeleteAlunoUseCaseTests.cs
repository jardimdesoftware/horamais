using Back.Application.UseCases.Aluno;
using Back.Application.Interfaces.Repositories;
using Moq;

namespace Back.Application.UnitTests.UseCases.Aluno;

public class DeleteAlunoUseCaseTests
{
    private readonly Mock<IAlunoRepository> _repo = new();

    [Fact]
    public async Task Deve_Chamar_Delete_No_Repositorio()
    {
        // Arrange
        var id = Guid.NewGuid();

        var useCase = new DeleteAlunoUseCase(_repo.Object);

        // Act
        await useCase.ExecuteAsync(id);

        // Assert
        _repo.Verify(r => r.DeleteComIdentityAsync(id), Times.Once);
    }
}
