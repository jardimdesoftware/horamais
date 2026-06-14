using Back.Domain.Entities.Certificado;
using FluentAssertions;

namespace Back.Domain.UnitTests;

public class CertificadoTests
{
    [Fact]
    public void Certificado_Valido_Deve_Passar()
    {
        var certificado = new CertificadoBuilder()
            .WithId(Guid.NewGuid())
            .WithTituloAtividade("Palestra")
            .WithInstituicao("IFPE")
            .WithLocal("Recife")
            .WithCategoria("Categoria X")
            .WithGrupo("Grupo A")
            .WithPeriodoLetivo("2024.1")
            .WithCargaHoraria(10)
            .WithDataInicio(DateTime.UtcNow.AddDays(-2))
            .WithDataFim(DateTime.UtcNow)
            .WithTotalPeriodos(1)
            .WithAnexoStorageKey("certificados/test.pdf")
            .WithTipo(TipoCertificado.COMPLEMENTAR)
            .WithAlunoAtividadeId(Guid.NewGuid())
            .Build();

        var results = ValidationHelper.ValidateObject(certificado);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Certificado_Sem_Titulo_Deve_Falhar()
    {
        var certificado = new CertificadoBuilder()
            .WithId(Guid.NewGuid())
            .WithInstituicao("IFPE")
            .WithLocal("Recife")
            .WithCategoria("Cat")
            .WithGrupo("Grupo")
            .WithPeriodoLetivo("2024.1")
            .WithCargaHoraria(10)
            .WithDataInicio(DateTime.UtcNow)
            .WithDataFim(DateTime.UtcNow)
            .WithTotalPeriodos(1)
            .WithAnexoStorageKey("certificados/test.pdf")
            .WithTipo(TipoCertificado.EXTENSAO)
            .WithAlunoAtividadeId(Guid.NewGuid())
            .Build();

        var results = ValidationHelper.ValidateObject(certificado);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(Certificado.TituloAtividade)));
    }

    [Fact]
    public void Certificado_Sem_Anexo_Deve_Falhar()
    {
        var certificado = new CertificadoBuilder()
            .WithId(Guid.NewGuid())
            .WithTituloAtividade("Teste")
            .WithInstituicao("IFPE")
            .WithLocal("Recife")
            .WithCategoria("Cat")
            .WithGrupo("Grupo")
            .WithPeriodoLetivo("2024.1")
            .WithCargaHoraria(10)
            .WithDataInicio(DateTime.UtcNow)
            .WithDataFim(DateTime.UtcNow)
            .WithTotalPeriodos(1)
            .WithTipo(TipoCertificado.EXTENSAO)
            .WithAlunoAtividadeId(Guid.NewGuid())
            .Build();

        var results = ValidationHelper.ValidateObject(certificado);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(Certificado.AnexoStorageKey)));
    }
}
