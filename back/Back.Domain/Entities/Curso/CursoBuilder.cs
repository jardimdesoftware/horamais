namespace Back.Domain.Entities.Curso;

public class CursoBuilder
{
    private readonly Curso _curso = new();

    public CursoBuilder WithId(Guid id)
    {
        _curso.Id = id;
        return this;
    }

    public CursoBuilder WithNome(string nome)
    {
        _curso.Nome = nome;
        return this;
    }

    public CursoBuilder WithCampusId(Guid campusId)
    {
        _curso.CampusId = campusId;
        return this;
    }

    public Curso Build() => _curso;
}
