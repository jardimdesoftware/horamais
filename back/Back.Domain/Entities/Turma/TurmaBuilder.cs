namespace Back.Domain.Entities.Turma;

public class TurmaBuilder
{
    private readonly Turma _turma = new();

    public TurmaBuilder WithId(Guid id)
    {
        _turma.Id = id;
        return this;
    }

    public TurmaBuilder WithPeriodo(string periodo)
    {
        _turma.Periodo = periodo;
        return this;
    }

    public TurmaBuilder WithTurno(string turno)
    {
        _turma.Turno = turno;
        return this;
    }

    public TurmaBuilder WithCodigo(string codigo)
    {
        _turma.Codigo = codigo;
        return this;
    }

    public TurmaBuilder WithCodigoAtivo(bool ativo)
    {
        _turma.CodigoAtivo = ativo;
        return this;
    }

    public TurmaBuilder WithPossuiExtensao(bool possui)
    {
        _turma.PossuiExtensao = possui;
        return this;
    }

    public TurmaBuilder WithMaximoHorasExtensao(int? horas)
    {
        _turma.MaximoHorasExtensao = horas;
        return this;
    }

    public TurmaBuilder WithCursoId(Guid cursoId)
    {
        _turma.CursoId = cursoId;
        return this;
    }

    public Turma Build() => _turma;
}
