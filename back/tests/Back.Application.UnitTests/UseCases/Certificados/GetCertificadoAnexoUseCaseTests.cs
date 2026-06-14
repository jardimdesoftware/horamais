using Back.Application.UseCases.Certificado;
using Back.Application.Interfaces.Repositories;
using Back.Application.Interfaces.Services;
using FluentAssertions;
using Moq;

namespace Back.Application.UnitTests.UseCases.Certificados;

public class GetCertificadoAnexoUseCaseTests
{
    private readonly Mock<ICertificadoRepository> _repo = new();
    private readonly Mock<IFileStorageService> _storage = new();

    private GetCertificadoAnexoUseCase CreateUseCase()
        => new(_repo.Object, _storage.Object);

    [Fact]
    public async Task Deve_Retornar_Anexo_Com_Sucesso()
    {
        var id = Guid.NewGuid();
        var storageKey = $"certificados/{id}.pdf";
        using var contentStream = new MemoryStream(new byte[] { 1, 2, 3 });

        _repo.Setup(r => r.GetStorageKeyByIdAsync(id))
            .ReturnsAsync(storageKey);
        _storage.Setup(s => s.DownloadAsync(storageKey))
            .ReturnsAsync((contentStream, "application/pdf"));

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(id);

        result.Content.Should().BeSameAs(contentStream);
        result.NomeArquivo.Should().Be($"certificado_{id}.pdf");
        result.ContentType.Should().Be("application/pdf");
    }

    [Fact]
    public async Task Deve_Lancar_Erro_Quando_Anexo_Inexistente()
    {
        var id = Guid.NewGuid();

        _repo.Setup(r => r.GetStorageKeyByIdAsync(id))
            .ReturnsAsync((string?)null);

        var useCase = CreateUseCase();

        var act = async () => await useCase.ExecuteAsync(id);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("Anexo não encontrado para este certificado.");
    }
}
