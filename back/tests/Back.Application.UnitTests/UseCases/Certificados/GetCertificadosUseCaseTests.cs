using Back.Application.UseCases.Certificado;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Certificado;
using Back.Domain.Entities.AlunoAtividade;
using FluentAssertions;
using Moq;

namespace Back.Application.UnitTests.UseCases.Certificados;

public class GetCertificadosUseCaseTests
{
    private readonly Mock<ICertificadoRepository> _repo = new();

    private GetCertificadosUseCase CreateUseCase()
        => new(_repo.Object);

    [Fact]
    public async Task Deve_Listar_Certificados()
    {
        var cert = new Certificado
        {
            Id = Guid.NewGuid(),
            TituloAtividade = "Cert",
            Instituicao = "IFPE",
            Local = "Campus",
            Categoria = "C",
            Grupo = "G",
            PeriodoLetivo = "2024.1",
            CargaHoraria = 10,
            DataInicio = DateTime.Today,
            DataFim = DateTime.Today,
            TotalPeriodos = 1,
            Tipo = TipoCertificado.COMPLEMENTAR,
            Status = StatusCertificado.PENDENTE,
            AlunoAtividade = new AlunoAtividade
            {
                AlunoId = Guid.NewGuid(),
                AtividadeId = Guid.NewGuid(),
                Atividade = new Back.Domain.Entities.Atividade.Atividade { CategoriaKey = "CK" }
            }
        };

        _repo.Setup(r => r.GetAsync(null, null))
            .ReturnsAsync(new List<Certificado> { cert });

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(null, null);

        result.Should().HaveCount(1);
    }
}
