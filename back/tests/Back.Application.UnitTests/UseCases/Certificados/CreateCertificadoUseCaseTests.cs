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
    private readonly Mock<Back.Application.Interfaces.Services.IFileStorageService> _storage = new();

    private CreateCertificadoUseCase CreateUseCase()
    {
        var validarLimite = new ValidarLimiteCertificadoUseCase(_certRepo.Object);
        return new(_alunoAtvRepo.Object, _certRepo.Object, _limiteRepo.Object, _atvRepo.Object, _storage.Object, validarLimite);
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

        var fileBytes = Encoding.UTF8.GetBytes("PDF");
        using var fileStream = new MemoryStream(fileBytes);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.ContentType).Returns("application/pdf");
        fileMock.Setup(f => f.FileName).Returns("file.pdf");
        fileMock.Setup(f => f.Length).Returns(fileBytes.Length);
        fileMock.Setup(f => f.OpenReadStream()).Returns(fileStream);

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
}
