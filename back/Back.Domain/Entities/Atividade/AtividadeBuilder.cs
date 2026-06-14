namespace Back.Domain.Entities.Atividade;

public class AtividadeBuilder
{
    private readonly Atividade _atividade = new();

    public AtividadeBuilder WithId(Guid id)
    {
        _atividade.Id = id;
        return this;
    }

    public AtividadeBuilder WithNome(string nome)
    {
        _atividade.Nome = nome;
        return this;
    }

    public AtividadeBuilder WithCargaMaximaSemestral(int carga)
    {
        _atividade.CargaMaximaSemestral = carga;
        return this;
    }

    public AtividadeBuilder WithCargaMaximaCurso(int carga)
    {
        _atividade.CargaMaximaCurso = carga;
        return this;
    }

    public AtividadeBuilder WithGrupo(string grupo)
    {
        _atividade.Grupo = grupo;
        return this;
    }

    public AtividadeBuilder WithTipo(TipoAtividade tipo)
    {
        _atividade.Tipo = tipo;
        return this;
    }

    public AtividadeBuilder WithCategoria(string categoria)
    {
        _atividade.Categoria = categoria;
        return this;
    }

    public AtividadeBuilder WithCategoriaKey(string categoriaKey)
    {
        _atividade.CategoriaKey = categoriaKey;
        return this;
    }

    public AtividadeBuilder WithPossuiCurricularizacaoExtensao(bool possui)
    {
        _atividade.PossuiCurricularizacaoExtensao = possui;
        return this;
    }

    public AtividadeBuilder WithHorasCurricularizacaoExtensao(int? horas)
    {
        _atividade.HorasCurricularizacaoExtensao = horas;
        return this;
    }

    public Atividade Build() => _atividade;
}
