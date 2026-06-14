namespace Back.Domain.Entities.Certificado;

public class CertificadoBuilder
{
    private readonly Certificado _certificado = new();

    public CertificadoBuilder()
    {
        _certificado.Status = StatusCertificado.PENDENTE;
    }

    public CertificadoBuilder WithId(Guid id)
    {
        _certificado.Id = id;
        return this;
    }

    public CertificadoBuilder WithTituloAtividade(string titulo)
    {
        _certificado.TituloAtividade = titulo;
        return this;
    }

    public CertificadoBuilder WithInstituicao(string instituicao)
    {
        _certificado.Instituicao = instituicao;
        return this;
    }

    public CertificadoBuilder WithLocal(string local)
    {
        _certificado.Local = local;
        return this;
    }

    public CertificadoBuilder WithCategoria(string categoria)
    {
        _certificado.Categoria = categoria;
        return this;
    }

    public CertificadoBuilder WithGrupo(string grupo)
    {
        _certificado.Grupo = grupo;
        return this;
    }

    public CertificadoBuilder WithPeriodoLetivo(string periodo)
    {
        _certificado.PeriodoLetivo = periodo;
        return this;
    }

    public CertificadoBuilder WithCargaHoraria(int carga)
    {
        _certificado.CargaHoraria = carga;
        return this;
    }

    public CertificadoBuilder WithDataInicio(DateTime inicio)
    {
        _certificado.DataInicio = inicio;
        return this;
    }

    public CertificadoBuilder WithDataFim(DateTime fim)
    {
        _certificado.DataFim = fim;
        return this;
    }

    public CertificadoBuilder WithTotalPeriodos(int total)
    {
        _certificado.TotalPeriodos = total;
        return this;
    }

    public CertificadoBuilder WithDescricao(string? descricao)
    {
        _certificado.Descricao = descricao;
        return this;
    }

    public CertificadoBuilder WithAnexoStorageKey(string storageKey)
    {
        _certificado.AnexoStorageKey = storageKey;
        return this;
    }

    public CertificadoBuilder WithTipo(TipoCertificado tipo)
    {
        _certificado.Tipo = tipo;
        return this;
    }

    public CertificadoBuilder WithAlunoAtividadeId(Guid alunoAtividadeId)
    {
        _certificado.AlunoAtividadeId = alunoAtividadeId;
        return this;
    }

    public Certificado Build() => _certificado;
}
