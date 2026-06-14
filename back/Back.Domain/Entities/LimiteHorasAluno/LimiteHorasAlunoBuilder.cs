namespace Back.Domain.Entities.LimiteHorasAluno;

public class LimiteHorasAlunoBuilder
{
    private readonly LimiteHorasAluno _limite = new();

    public LimiteHorasAlunoBuilder WithId(Guid id)
    {
        _limite.Id = id;
        return this;
    }

    public LimiteHorasAlunoBuilder WithMaximoHorasComplementar(int horas)
    {
        _limite.MaximoHorasComplementar = horas;
        return this;
    }

    public LimiteHorasAlunoBuilder WithCursoId(Guid cursoId)
    {
        _limite.CursoId = cursoId;
        return this;
    }

    public LimiteHorasAluno Build() => _limite;
}
