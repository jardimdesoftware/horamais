using Back.Domain.Entities.Aluno;
using FluentAssertions;

namespace Back.Domain.UnitTests;

public class AlunoTests
{
    [Fact]
    public void Aluno_Valido_Deve_Passar()
    {
        var aluno = new AlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("João")
            .WithEmail("joao@teste.com")
            .WithMatricula("20230001")
            .WithTurmaId(Guid.NewGuid())
            .WithIdentityUserId("abc123")
            .Build();

        var results = ValidationHelper.ValidateObject(aluno);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Aluno_Com_Email_Invalido_Deve_Falhar()
    {
        var aluno = new AlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("João")
            .WithEmail("email_invalido")
            .WithMatricula("20230001")
            .WithTurmaId(Guid.NewGuid())
            .WithIdentityUserId("abc123")
            .Build();

        var results = ValidationHelper.ValidateObject(aluno);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(Aluno.Email)));
    }

    [Fact]
    public void Aluno_Sem_Nome_Deve_Falhar()
    {
        var aluno = new AlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("joao@teste.com")
            .WithMatricula("20230001")
            .WithTurmaId(Guid.NewGuid())
            .WithIdentityUserId("abc123")
            .Build();

        var results = ValidationHelper.ValidateObject(aluno);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(Aluno.Nome)));
    }
}
