namespace Back.Domain.Entities.Coordenador;

public class CoordenadorBuilder
{
    private readonly Coordenador _coordenador = new();

    public CoordenadorBuilder WithId(Guid id)
    {
        _coordenador.Id = id;
        return this;
    }

    public CoordenadorBuilder WithNome(string nome)
    {
        _coordenador.Nome = nome;
        return this;
    }

    public CoordenadorBuilder WithNumeroPortaria(string numeroPortaria)
    {
        _coordenador.NumeroPortaria = numeroPortaria;
        return this;
    }

    public CoordenadorBuilder WithDOU(string dou)
    {
        _coordenador.DOU = dou;
        return this;
    }

    public CoordenadorBuilder WithEmail(string email)
    {
        _coordenador.Email = email;
        return this;
    }

    public CoordenadorBuilder WithCursoId(Guid cursoId)
    {
        _coordenador.CursoId = cursoId;
        return this;
    }

    public CoordenadorBuilder WithIdentityUserId(string identityUserId)
    {
        _coordenador.IdentityUserId = identityUserId;
        return this;
    }

    public Coordenador Build() => _coordenador;
}
