using Back.Application.DTOs.Atividade;
using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Atividade;
using FluentAssertions;
using Moq;

using DomainAtividade = Back.Domain.Entities.Atividade.Atividade;
using TipoAtividade = Back.Domain.Entities.Atividade.TipoAtividade;

namespace Back.Application.UnitTests.UseCases.Atividade;

public class UpdateAtividadeUseCaseTests
{
    [Fact]
    public async Task Deve_Atualizar_Atividade()
    {
        var repo = new Mock<IAtividadeRepository>();

        var atividade = new DomainAtividade
        {
            Id = Guid.NewGuid(),
            Nome = "Old",
            Grupo = "G1",
            Categoria = "C1",
            CategoriaKey = "CK1",
            CargaMaximaCurso = 30,
            CargaMaximaSemestral = 10,
            Tipo = TipoAtividade.COMPLEMENTAR
        };

        repo.Setup(r => r.GetByIdAsync(atividade.Id)).ReturnsAsync(atividade);

        var request = new UpdateAtividadeRequest
        {
            Nome = "Nova",
            Grupo = "GX",
            Categoria = "CX",
            CategoriaKey = "KX",
            CargaMaximaSemestral = 20,
            CargaMaximaCurso = 60,
            Tipo = TipoAtividade.EXTENSAO
        };

        var useCase = new UpdateAtividadeUseCase(repo.Object);

        await useCase.ExecuteAsync(atividade.Id, request);

        atividade.Nome.Should().Be("Nova");
        atividade.Tipo.Should().Be(TipoAtividade.EXTENSAO);

        repo.Verify(r => r.UpdateAsync(atividade), Times.Once);
    }
}
