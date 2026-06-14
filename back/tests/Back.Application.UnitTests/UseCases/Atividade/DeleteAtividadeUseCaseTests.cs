using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Atividade;
using Moq;

using DomainAtividade = Back.Domain.Entities.Atividade.Atividade;
using DomainAlunoAtividade = Back.Domain.Entities.AlunoAtividade.AlunoAtividade;

namespace Back.Application.UnitTests.UseCases.Atividade;

public class DeleteAtividadeUseCaseTests
{
    [Fact]
    public async Task Deve_Deletar_Atividade_E_Suas_Relacoes()
    {
        var atividadeRepo = new Mock<IAtividadeRepository>();
        var alunoAtividadeRepo = new Mock<IAlunoAtividadeRepository>();

        var atividade = new DomainAtividade { Id = Guid.NewGuid(), Nome = "Teste" };

        atividadeRepo.Setup(r => r.GetByIdAsync(atividade.Id))
                     .ReturnsAsync(atividade);

        alunoAtividadeRepo.Setup(r => r.GetByAtividadeIdAsync(atividade.Id))
                          .ReturnsAsync(new List<DomainAlunoAtividade>
                          {
                              new DomainAlunoAtividade { Id = Guid.NewGuid() }
                          });

        var useCase = new DeleteAtividadeUseCase(atividadeRepo.Object, alunoAtividadeRepo.Object);

        await useCase.ExecuteAsync(atividade.Id);

        alunoAtividadeRepo.Verify(r => r.RemoveRangeAsync(It.IsAny<IEnumerable<DomainAlunoAtividade>>()), Times.Once);
        atividadeRepo.Verify(r => r.DeleteAsync(atividade), Times.Once);
    }
}
