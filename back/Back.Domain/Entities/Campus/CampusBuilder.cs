namespace Back.Domain.Entities.Campus;

public class CampusBuilder
{
    private readonly Campus _campus = new();

    public CampusBuilder WithId(Guid id)
    {
        _campus.Id = id;
        return this;
    }

    public CampusBuilder WithNome(string nome)
    {
        _campus.Nome = nome;
        return this;
    }

    public CampusBuilder WithCidade(string cidade)
    {
        _campus.Cidade = cidade;
        return this;
    }

    public Campus Build() => _campus;
}
