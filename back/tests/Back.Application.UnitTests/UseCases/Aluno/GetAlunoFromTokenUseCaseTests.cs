using System.Security.Claims;
using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Aluno;
using Back.Domain.Entities.Aluno;
using Back.Domain.Entities.AlunoAtividade;
using Back.Domain.Entities.Atividade;
using Back.Domain.Entities.LimiteHorasAluno;
using Back.Domain.Entities.Turma;
using FluentAssertions;
using Moq;
using DomainAtividade = Back.Domain.Entities.Atividade.Atividade;

namespace Back.Application.UnitTests.UseCases.Aluno;

public class GetAlunoFromTokenUseCaseTests
{
    private readonly Mock<IAlunoRepository> _alunoRepo = new();
    private readonly Mock<ILimiteHorasAlunoRepository> _limiteRepo = new();

    private static void SetPrivate<T, V>(T obj, string prop, V value)
    {
        var p = typeof(T).GetProperty(
            prop,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);

        p!.SetValue(obj, value);
    }

    private ClaimsPrincipal CreateUser(string id)
        => new ClaimsPrincipal(new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, id)
        }));

    [Fact]
    public async Task Deve_Retornar_Dados_Do_Aluno_Via_Token()
    {
        // Arrange
        var identityId = "user-123";
        var cursoId = Guid.NewGuid();

        var turma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithPeriodo("2024.1")
            .WithTurno("Manhã")
            .WithCursoId(cursoId)
            .WithMaximoHorasExtensao(10)
            .Build();

        var aluno = new AlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("Aluno Token")
            .WithEmail("token@ifpe.edu.br")
            .WithMatricula("0001")
            .WithTurmaId(turma.Id)
            .WithIdentityUserId(identityId)
            .Build();

        // Setter privado
        SetPrivate(aluno, nameof(aluno.Turma), turma);

        var atividade = new DomainAtividade
        {
            Id = Guid.NewGuid(),
            Nome = "Ativ X",
            Grupo = "G",
            Categoria = "CAT",
            CategoriaKey = "CK",
            Tipo = TipoAtividade.EXTENSAO,
            CargaMaximaCurso = 40,
            CargaMaximaSemestral = 10
        };

        var alunoAtiv = new AlunoAtividadeBuilder()
            .WithId(Guid.NewGuid())
            .WithAlunoId(aluno.Id)
            .WithAtividadeId(atividade.Id)
            .WithHorasConcluidas(15)
            .Build();

        alunoAtiv.Atividade = atividade;
        aluno.Atividades.Add(alunoAtiv);

        var limite = new LimiteHorasAluno
        {
            Id = Guid.NewGuid(),
            CursoId = cursoId,
            MaximoHorasComplementar = 20
        };

        _alunoRepo.Setup(r => r.GetByIdentityUserIdWithAtividadesAsync(identityId))
            .ReturnsAsync(aluno);

        _limiteRepo.Setup(r => r.GetByCursoIdAsync(cursoId))
            .ReturnsAsync(limite);

        var useCase = new GetAlunoFromTokenUseCase(
            _alunoRepo.Object,
            _limiteRepo.Object
        );

        // Act
        var result = await useCase.ExecuteAsync(CreateUser(identityId));

        // Assert
        result.Email.Should().Be("token@ifpe.edu.br");

        result.TotalHorasExtensao.Should().Be(15);
        result.MaximoHorasExtensao.Should().Be(10);

        result.TotalHorasComplementar.Should().Be(0);
        result.MaximoHorasComplementar.Should().Be(20);
    }
}
