using Back.Application.UseCases.Certificado;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Certificado;
using Back.Domain.Entities.AlunoAtividade;
using FluentAssertions;
using Moq;

namespace Back.Application.UnitTests.UseCases.Certificados;

public class GetCertificadoByIdUseCaseTests
{
    private readonly Mock<ICertificadoRepository> _repo = new();

    private GetCertificadoByIdUseCase CreateUseCase()
        => new(_repo.Object);

    [Fact]
    public async Task Deve_Retornar_Detalhes_Do_Certificado()
    {
        var cert = new Certificado
        {
            Id = Guid.NewGuid(),
            TituloAtividade = "Curso X",
            Instituicao = "IFPE",
            Local = "Campus",
            Categoria = "Cat",
            Grupo = "G1",
            PeriodoLetivo = "2024.1",
            CargaHoraria = 20,
            DataInicio = DateTime.Today,
            DataFim = DateTime.Today,
            TotalPeriodos = 1,
            Tipo = TipoCertificado.COMPLEMENTAR,
            Status = StatusCertificado.PENDENTE,
            AlunoAtividade = new AlunoAtividade
            {
                AlunoId = Guid.NewGuid(),
                AtividadeId = Guid.NewGuid()
            }
        };

        _repo.Setup(r => r.GetByIdWithAlunoAtividadeAsync(cert.Id))
            .ReturnsAsync(cert);

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(cert.Id);

        result.Id.Should().Be(cert.Id);
    }

    [Fact]
    public async Task Deve_Lancar_Erro_Quando_Certificado_Nao_Existe()
    {
        _repo.Setup(r => r.GetByIdWithAlunoAtividadeAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Certificado?)null);

        var useCase = CreateUseCase();

        var act = async () => await useCase.ExecuteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }
}
