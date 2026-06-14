using System.Security.Claims;
using Back.Application.UseCases.Aluno;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Aluno;
using Back.Domain.Entities.AlunoAtividade;
using Back.Domain.Entities.Atividade;
using Back.Domain.Entities.Coordenador;
using Back.Domain.Entities.Curso;
using Back.Domain.Entities.LimiteHorasAluno;
using Back.Domain.Entities.Turma;
using FluentAssertions;
using Moq;
using DomainAtividade = Back.Domain.Entities.Atividade.Atividade;

namespace Back.Application.UnitTests.UseCases.Aluno;

public class ContarPendenciasDownloadUseCaseTests
{
    private readonly Mock<IAlunoRepository> _alunoRepo = new();
    private readonly Mock<ILimiteHorasAlunoRepository> _limiteRepo = new();
    private readonly Mock<ICoordenadorRepository> _coordenadorRepo = new();

    private ContarPendenciasDownloadUseCase CreateUseCase()
        => new ContarPendenciasDownloadUseCase(
            _alunoRepo.Object,
            _limiteRepo.Object,
            _coordenadorRepo.Object);

    private ClaimsPrincipal CreateUser(string id)
        => new ClaimsPrincipal(
            new ClaimsIdentity(new[] {
                new Claim(ClaimTypes.NameIdentifier, id)
            })
        );

    // Helper para setar propriedades com setter private
    private static void SetPrivateProperty<T, TValue>(T obj, string propertyName, TValue value)
    {
        var prop = typeof(T).GetProperty(propertyName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);

        prop!.SetValue(obj, value);
    }

    [Fact]
    public async Task Deve_Retornar_Contagem_Correta()
    {
        // Arrange
        var identityUserId = "coord-123";

        var coordenador = new CoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("Coord")
            .WithEmail("coord@ifpe.edu.br")
            .WithNumeroPortaria("123")
            .WithDOU("dou")
            .WithIdentityUserId(identityUserId)
            .WithCursoId(Guid.NewGuid())
            .Build();

        // Criando curso
        var curso = new Curso
        {
            Id = coordenador.CursoId,
            Nome = "Curso Teste"
        };

        // Injetando Curso via reflection
        SetPrivateProperty(coordenador, "Curso", curso);

        var turma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithCursoId(curso.Id)
            .Build();

        var aluno = new AlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("Aluno 1")
            .WithEmail("a1@ifpe.edu.br")
            .WithMatricula("001")
            .WithTurmaId(turma.Id)
            .WithIdentityUserId("al1")
            .Build();

        SetPrivateProperty(aluno, nameof(aluno.Turma), turma);

        var atividadeAluno = new AlunoAtividadeBuilder()
            .WithId(Guid.NewGuid())
            .WithAlunoId(aluno.Id)
            .WithAtividadeId(Guid.NewGuid())
            .WithHorasConcluidas(10)
            .Build();

        atividadeAluno.Atividade = new DomainAtividade
        {
            Id = Guid.NewGuid(),
            Nome = "Ativ",
            Categoria = "C",
            CategoriaKey = "CK",
            Grupo = "G",
            CargaMaximaCurso = 40,
            CargaMaximaSemestral = 10,
            Tipo = TipoAtividade.COMPLEMENTAR
        };

        aluno.Atividades.Add(atividadeAluno);

        var limite = new LimiteHorasAluno
        {
            Id = Guid.NewGuid(),
            CursoId = curso.Id,
            MaximoHorasComplementar = 5
        };

        _coordenadorRepo.Setup(r => r.GetByIdentityUserIdWithCursoAsync(identityUserId))
            .ReturnsAsync(coordenador);

        _alunoRepo.Setup(r => r.GetAlunosPorCursoComDetalhesAsync(curso.Id))
            .ReturnsAsync(new List<Back.Domain.Entities.Aluno.Aluno> { aluno });

        _limiteRepo.Setup(r => r.GetByCursoIdAsync(curso.Id))
            .ReturnsAsync(limite);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.ExecuteAsync(CreateUser(identityUserId));

        // Assert
        result.TotalPendencias.Should().Be(1);
    }
}
