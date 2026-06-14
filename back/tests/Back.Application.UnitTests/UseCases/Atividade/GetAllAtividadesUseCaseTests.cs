using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Atividade;
using FluentAssertions;
using Moq;

using DomainAtividade = Back.Domain.Entities.Atividade.Atividade;
using TipoAtividade = Back.Domain.Entities.Atividade.TipoAtividade;

namespace Back.Application.UnitTests.UseCases.Atividade;

public class GetAllAtividadesUseCaseTests
{
    [Fact]
    public async Task Deve_Retornar_Todas_Atividades()
    {
        var repo = new Mock<IAtividadeRepository>();

        repo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<DomainAtividade>
            {
                new DomainAtividade
                {
                    Id = Guid.NewGuid(),
                    Nome = "A1",
                    Grupo = "G1",
                    Categoria = "CAT",
                    CategoriaKey = "CK",
                    CargaMaximaCurso = 40,
                    CargaMaximaSemestral = 10,
                    Tipo = TipoAtividade.EXTENSAO
                }
            });

        var useCase = new GetAllAtividadesUseCase(repo.Object);

        var result = (await useCase.ExecuteAsync()).ToList();

        result.Should().HaveCount(1);
        result[0].Nome.Should().Be("A1");
    }
}
