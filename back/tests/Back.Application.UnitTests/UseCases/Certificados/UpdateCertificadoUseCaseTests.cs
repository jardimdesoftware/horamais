using Back.Application.UseCases.Certificado;
using Back.Application.Interfaces.Repositories;
using Back.Application.DTOs.Certificado;
using Back.Domain.Entities.Certificado;
using Back.Domain.Entities.Aluno;
using FluentAssertions;
using Moq;

namespace Back.Application.UnitTests.UseCases.Certificados;

public class UpdateCertificadoUseCaseTests
{
    private readonly Mock<ICertificadoRepository> _repo = new();
    private readonly Mock<IAlunoRepository> _alunoRepo = new();
    private readonly Mock<Back.Application.Interfaces.Services.IFileStorageService> _storage = new();

    private UpdateCertificadoUseCase CreateUseCase()
    {
        var validarLimite = new ValidarLimiteCertificadoUseCase(_repo.Object);
        return new(_repo.Object, _alunoRepo.Object, _storage.Object, validarLimite);
    }

    private (Certificado cert, string identityUserId) BuildOwnerSetup()
    {
        var alunoId = Guid.NewGuid();
        var identityUserId = "user-identity-123";
        var atividade = new Back.Domain.Entities.Atividade.Atividade
        {
            Id = Guid.NewGuid(),
            CargaMaximaSemestral = 100,
            CargaMaximaCurso = 300
        };
        var alunoAtividade = new Back.Domain.Entities.AlunoAtividade.AlunoAtividade
        {
            AlunoId = alunoId,
            Atividade = atividade
        };
        var cert = new Certificado
        {
            Id = Guid.NewGuid(),
            AlunoAtividade = alunoAtividade
        };

        _alunoRepo.Setup(r => r.GetByIdentityUserIdAsync(identityUserId))
            .ReturnsAsync(new AlunoBuilder().WithId(alunoId).Build());

        return (cert, identityUserId);
    }

    [Fact]
    public async Task Deve_Atualizar_Certificado_Quando_Pendente()
    {
        var (cert, identityUserId) = BuildOwnerSetup();
        cert.Status = StatusCertificado.PENDENTE;
        cert.TituloAtividade = "Old";

        _repo.Setup(r => r.GetByIdAsync(cert.Id))
            .ReturnsAsync(cert);
        _repo.Setup(r => r.GetByAlunoAtividadeAndStatusAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<StatusCertificado>>()))
            .ReturnsAsync([]);

        var req = new UpdateCertificadoRequest
        {
            TituloAtividade = "New",
            Instituicao = "IFPE",
            Local = "Campus",
            Categoria = "Cat",
            Grupo = "G",
            PeriodoLetivo = "2024.1",
            CargaHoraria = 10,
            DataInicio = DateTime.Today,
            DataFim = DateTime.Today,
            TotalPeriodos = 1,
            Tipo = Back.Domain.Entities.Atividade.TipoAtividade.COMPLEMENTAR
        };

        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(cert.Id, req, identityUserId);

        cert.TituloAtividade.Should().Be("New");
        _repo.Verify(r => r.UpdateAsync(cert), Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Atualizar_Certificado_Aprovado()
    {
        var (cert, identityUserId) = BuildOwnerSetup();
        cert.Status = StatusCertificado.APROVADO;

        _repo.Setup(r => r.GetByIdAsync(cert.Id))
            .ReturnsAsync(cert);

        var req = new UpdateCertificadoRequest
        {
            TituloAtividade = "X",
            Instituicao = "Y",
            Local = "Z",
            Categoria = "C",
            Grupo = "G",
            PeriodoLetivo = "2024.1",
            CargaHoraria = 10,
            DataInicio = DateTime.Today,
            DataFim = DateTime.Today,
            TotalPeriodos = 1,
            Tipo = Back.Domain.Entities.Atividade.TipoAtividade.COMPLEMENTAR
        };

        var useCase = CreateUseCase();

        var act = async () => await useCase.ExecuteAsync(cert.Id, req, identityUserId);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Não é possível alterar um certificado que já foi APROVADO.");
    }

    [Fact]
    public async Task Nao_Deve_Atualizar_Certificado_De_Outro_Aluno()
    {
        var (cert, _) = BuildOwnerSetup();
        cert.Status = StatusCertificado.PENDENTE;

        _repo.Setup(r => r.GetByIdAsync(cert.Id))
            .ReturnsAsync(cert);

        var outroIdentityUserId = "outro-user-456";
        _alunoRepo.Setup(r => r.GetByIdentityUserIdAsync(outroIdentityUserId))
            .ReturnsAsync(new AlunoBuilder().WithId(Guid.NewGuid()).Build()); // ID diferente

        var req = new UpdateCertificadoRequest
        {
            TituloAtividade = "X", Instituicao = "Y", Local = "Z",
            Categoria = "C", Grupo = "G", PeriodoLetivo = "2024.1",
            CargaHoraria = 10, DataInicio = DateTime.Today, DataFim = DateTime.Today,
            TotalPeriodos = 1, Tipo = Back.Domain.Entities.Atividade.TipoAtividade.COMPLEMENTAR
        };

        var useCase = CreateUseCase();

        var act = async () => await useCase.ExecuteAsync(cert.Id, req, outroIdentityUserId);

        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Você não tem permissão para alterar este certificado.");
    }
}
