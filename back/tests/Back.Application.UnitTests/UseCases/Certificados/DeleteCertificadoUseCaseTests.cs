using Back.Application.UseCases.Certificado;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Certificado;
using FluentAssertions;
using Moq;

namespace Back.Application.UnitTests.UseCases.Certificados;

public class DeleteCertificadoUseCaseTests
{
    private readonly Mock<ICertificadoRepository> _repo = new();
    private readonly Mock<Back.Application.Interfaces.Services.IFileStorageService> _storage = new();

    private DeleteCertificadoUseCase CreateUseCase()
        => new(_repo.Object, _storage.Object);

    [Fact]
    public async Task Deve_Deletar_Certificado_Quando_Pendente()
    {
        var cert = new Certificado
        {
            Id = Guid.NewGuid(),
            Status = StatusCertificado.PENDENTE
        };

        _repo.Setup(r => r.GetByIdAsync(cert.Id)).ReturnsAsync(cert);

        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(cert.Id);

        _repo.Verify(r => r.DeleteAsync(cert), Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Deletar_Certificado_Aprovado()
    {
        var cert = new Certificado
        {
            Id = Guid.NewGuid(),
            Status = StatusCertificado.APROVADO
        };

        _repo.Setup(r => r.GetByIdAsync(cert.Id)).ReturnsAsync(cert);

        var useCase = CreateUseCase();

        var act = async () => await useCase.ExecuteAsync(cert.Id);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Não é possível excluir um certificado que já foi APROVADO.");
    }
}
