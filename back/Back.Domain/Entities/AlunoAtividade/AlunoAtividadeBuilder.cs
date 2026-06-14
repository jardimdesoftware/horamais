namespace Back.Domain.Entities.AlunoAtividade;

public class AlunoAtividadeBuilder
{
    private readonly AlunoAtividade _alunoAtividade = new();

    public AlunoAtividadeBuilder WithAlunoId(Guid alunoId)
    {
        _alunoAtividade.AlunoId = alunoId;
        return this;
    }

    public AlunoAtividadeBuilder WithAtividadeId(Guid atividadeId)
    {
        _alunoAtividade.AtividadeId = atividadeId;
        return this;
    }

    public AlunoAtividadeBuilder WithHorasConcluidas(int horas)
    {
        _alunoAtividade.HorasConcluidas = horas;
        return this;
    }

    public AlunoAtividadeBuilder WithId(Guid id)
    {
        _alunoAtividade.Id = id;
        return this;
    }

    public AlunoAtividade Build() => _alunoAtividade;
}
