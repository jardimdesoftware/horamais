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

public class GetAlunoDetalhadoUseCaseTests
{
    private readonly Mock<IAlunoRepository> _alunoRepo = new();
    private readonly Mock<IAlunoAtividadeRepository> _atividadeRepo = new();
    private readonly Mock<ILimiteHorasAlunoRepository> _limiteRepo = new();
    private readonly Mock<IAtividadeRepository> _atividadeCursoRepo = new();

    private static void SetPrivateProperty<T, TValue>(T obj, string propertyName, TValue value)
    {
        var prop = typeof(T).GetProperty(
            propertyName,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public
        );

        prop!.SetValue(obj, value);
    }

    [Fact]
    public async Task Deve_Retornar_Aluno_Detalhado()
    {
        // Arrange
        var alunoId = Guid.NewGuid();
        var cursoId = Guid.NewGuid();

        var turma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithPeriodo("2024.1")
            .WithTurno("Noite")
            .WithCursoId(cursoId)
            .WithMaximoHorasExtensao(5)
            .Build();

        var aluno = new AlunoBuilder()
            .WithId(alunoId)
            .WithNome("Aluno Teste")
            .WithEmail("aluno@ifpe.edu.br")
            .WithMatricula("2023001")
            .WithTurmaId(turma.Id)
            .WithIdentityUserId("uid")
            .Build();

        // Injeta a turma via reflection
        SetPrivateProperty(aluno, nameof(aluno.Turma), turma);

        var atividade = new DomainAtividade
        {
            Id = Guid.NewGuid(),
            Nome = "Oficina",
            Grupo = "G1",
            Categoria = "C1",
            CategoriaKey = "CK1",
            Tipo = TipoAtividade.COMPLEMENTAR,
            CargaMaximaCurso = 40,
            CargaMaximaSemestral = 10
        };

        var alunoAtiv = new AlunoAtividadeBuilder()
            .WithId(Guid.NewGuid())
            .WithAlunoId(alunoId)
            .WithAtividadeId(atividade.Id)
            .WithHorasConcluidas(12)
            .Build();

        alunoAtiv.Atividade = atividade;
        aluno.Atividades.Add(alunoAtiv);

        var limite = new LimiteHorasAluno
        {
            Id = Guid.NewGuid(),
            CursoId = cursoId,
            MaximoHorasComplementar = 10
        };

        _alunoRepo.Setup(r => r.GetByIdWithAtividadesAsync(alunoId))
            .ReturnsAsync(aluno);

        _limiteRepo.Setup(r => r.GetByCursoIdAsync(cursoId))
            .ReturnsAsync(limite);

        // Atividades são globais: retorna todas
        _atividadeCursoRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<DomainAtividade> { atividade });

        var useCase = new GetAlunoDetalhadoUseCase(
            _alunoRepo.Object,
            _atividadeRepo.Object,
            _limiteRepo.Object,
            _atividadeCursoRepo.Object // AGORA SIM
        );

        // Act
        var result = await useCase.ExecuteAsync(alunoId);

        // Assert
        result.Should().NotBeNull();
        result.Nome.Should().Be("Aluno Teste");

        result.TotalHorasComplementar.Should().Be(12);
        result.MaximoHorasComplementar.Should().Be(10);

        result.TotalHorasExtensao.Should().Be(0);
        result.MaximoHorasExtensao.Should().Be(5);
    }
}
