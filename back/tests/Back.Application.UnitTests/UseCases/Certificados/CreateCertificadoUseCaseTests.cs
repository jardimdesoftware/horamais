using Back.Application.DTOs.Certificado;
using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Certificado;
using Back.Domain.Entities.AlunoAtividade;
using Back.Domain.Entities.Certificado;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;

namespace Back.Application.UnitTests.UseCases.Certificados;

public class CreateCertificadoUseCaseTests
{
    private readonly Mock<IAlunoAtividadeRepository> _alunoAtvRepo = new();
    private readonly Mock<ICertificadoRepository> _certRepo = new();
    private readonly Mock<ILimiteHorasAlunoRepository> _limiteRepo = new();
    private readonly Mock<IAtividadeRepository> _atvRepo = new();
    private readonly Mock<IAlunoRepository> _alunoRepo = new();
    private readonly Mock<Back.Application.Interfaces.Services.IFileStorageService> _storage = new();

    private CreateCertificadoUseCase CreateUseCase()
    {
        var validarLimite = new ValidarLimiteCertificadoUseCase(_certRepo.Object);
        return new(_alunoAtvRepo.Object, _certRepo.Object, _limiteRepo.Object, _atvRepo.Object, _alunoRepo.Object, _storage.Object, validarLimite);
    }

    private static Mock<IFormFile> CriarAnexoMock(out MemoryStream stream)
    {
        var fileBytes = Encoding.UTF8.GetBytes("PDF");
        stream = new MemoryStream(fileBytes);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");
        fileMock.Setup(f => f.FileName).Returns("file.pdf");
        fileMock.Setup(f => f.Length).Returns(fileBytes.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        return fileMock;
    }

    [Fact]
    public async Task Deve_Criar_Certificado_Com_Sucesso()
    {
        // Arrange
        var atividade = new Back.Domain.Entities.Atividade.Atividade
        {
            Id = Guid.NewGuid(),
            CargaMaximaSemestral = 100,
            CargaMaximaCurso = 300
        };
        var alunoAtv = new AlunoAtividade { Id = Guid.NewGuid(), Atividade = atividade };
        _alunoAtvRepo.Setup(r => r.GetByAlunoEAtividadeAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
            .ReturnsAsync(alunoAtv);
        _certRepo.Setup(r => r.GetByAlunoAtividadeAndStatusAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Back.Domain.Entities.Certificado.StatusCertificado>>()))
            .ReturnsAsync([]);
        _storage.Setup(s => s.UploadAsync(It.IsAny<IFormFile>(), It.IsAny<string>()))
            .ReturnsAsync((IFormFile _, string key) => key);
        _alunoRepo.Setup(r => r.GetPeriodoIngressoAsync(It.IsAny<Guid>()))
            .ReturnsAsync("2023.1");

        var fileMock = CriarAnexoMock(out var fileStream);
        using var _ = fileStream;

        var req = new CreateCertificadoRequest
        {
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
            Anexo = fileMock.Object,
            Tipo = Back.Domain.Entities.Certificado.TipoCertificado.COMPLEMENTAR,
            AlunoId = Guid.NewGuid(),
            AtividadeId = Guid.NewGuid()
        };

        var useCase = CreateUseCase();

        // Act
        var id = await useCase.ExecuteAsync(req);

        // Assert
        id.Should().NotBeEmpty();
        _certRepo.Verify(r => r.AddAsync(It.IsAny<Certificado>()), Times.Once);
    }

    [Fact]
    public async Task Deve_Rejeitar_Periodo_Anterior_Ao_Ingresso()
    {
        // Arrange: aluno ingressou em 2023.2, tenta registrar em 2022.1
        _alunoRepo.Setup(r => r.GetPeriodoIngressoAsync(It.IsAny<Guid>()))
            .ReturnsAsync("2023.2");

        var fileMock = CriarAnexoMock(out var fileStream);
        using var _ = fileStream;

        var req = NovaRequest(fileMock.Object, periodo: "2022.1");
        var useCase = CreateUseCase();

        // Act
        var act = async () => await useCase.ExecuteAsync(req);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*anterior ao seu ingresso*");
        _certRepo.Verify(r => r.AddAsync(It.IsAny<Certificado>()), Times.Never);
    }

    [Fact]
    public async Task Deve_Rejeitar_Periodo_Anterior_A_2019_2()
    {
        // Arrange: sem ingresso conhecido, período abaixo do mínimo do sistema
        _alunoRepo.Setup(r => r.GetPeriodoIngressoAsync(It.IsAny<Guid>()))
            .ReturnsAsync((string?)null);

        var fileMock = CriarAnexoMock(out var fileStream);
        using var _ = fileStream;

        var req = NovaRequest(fileMock.Object, periodo: "2019.1");
        var useCase = CreateUseCase();

        // Act
        var act = async () => await useCase.ExecuteAsync(req);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*2019.2*");
        _certRepo.Verify(r => r.AddAsync(It.IsAny<Certificado>()), Times.Never);
    }

    private static CreateCertificadoRequest NovaRequest(IFormFile anexo, string periodo) => new()
    {
        TituloAtividade = "Curso X",
        Instituicao = "IFPE",
        Local = "Campus",
        Categoria = "Cat",
        Grupo = "G1",
        PeriodoLetivo = periodo,
        CargaHoraria = 20,
        DataInicio = DateTime.Today,
        DataFim = DateTime.Today,
        TotalPeriodos = 1,
        Anexo = anexo,
        Tipo = Back.Domain.Entities.Certificado.TipoCertificado.COMPLEMENTAR,
        AlunoId = Guid.NewGuid(),
        AtividadeId = Guid.NewGuid()
    };
}
