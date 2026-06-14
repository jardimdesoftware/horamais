namespace Back.Domain.Entities.Aluno;

public class AlunoBuilder
{
    private readonly Aluno _aluno = new();

    public AlunoBuilder()
    {
        _aluno.JaBaixadoHorasComplementares = false;
        _aluno.JaBaixadoHorasExtensao = false;
        _aluno.IsAtivo = true;
    }

    public AlunoBuilder WithId(Guid id)
    {
        _aluno.Id = id;
        return this;
    }

    public AlunoBuilder WithNome(string nome)
    {
        _aluno.Nome = nome;
        return this;
    }

    public AlunoBuilder WithEmail(string email)
    {
        _aluno.Email = email;
        return this;
    }

    public AlunoBuilder WithMatricula(string matricula)
    {
        _aluno.Matricula = matricula;
        return this;
    }

    public AlunoBuilder WithTurmaId(Guid turmaId)
    {
        _aluno.TurmaId = turmaId;
        return this;
    }

    public AlunoBuilder WithIdentityUserId(string identityUserId)
    {
        _aluno.IdentityUserId = identityUserId;
        return this;
    }

    public AlunoBuilder WithIsAtivo(bool isAtivo)
    {
        _aluno.IsAtivo = isAtivo;
        return this;
    }

    public Aluno Build() => _aluno;
}
