namespace Back.Domain.Entities.LembreteEmail;

public class LembreteEmailBuilder
{
    private readonly LembreteEmail _lembrete = new();

    public LembreteEmailBuilder WithId(Guid id)
    {
        _lembrete.Id = id;
        return this;
    }

    public LembreteEmailBuilder WithCursoId(Guid cursoId)
    {
        _lembrete.CursoId = cursoId;
        return this;
    }

    public LembreteEmailBuilder WithData(DateTime data)
    {
        _lembrete.Data = data;
        return this;
    }

    public LembreteEmailBuilder WithMensagemPersonalizada(string? mensagem)
    {
        _lembrete.MensagemPersonalizada = mensagem;
        return this;
    }

    public LembreteEmail Build() => _lembrete;
}
