namespace Back.Domain.Entities.Convite;

public class ConviteCoordenadorBuilder
{
    private readonly ConviteCoordenador _convite = new();

    public ConviteCoordenadorBuilder WithEmail(string email)
    {
        _convite.Email = email;
        return this;
    }

    public ConviteCoordenadorBuilder WithCursoId(Guid cursoId)
    {
        _convite.CursoId = cursoId;
        return this;
    }

    public ConviteCoordenadorBuilder WithToken(string token)
    {
        _convite.Token = token;
        return this;
    }

    public ConviteCoordenadorBuilder WithExpiracao(DateTime data)
    {
        _convite.ExpiraEm = data;
        return this;
    }

    public ConviteCoordenadorBuilder WithId(Guid id)
    {
        _convite.Id = id;
        return this;
    }

    public ConviteCoordenador Build() => _convite;
}
