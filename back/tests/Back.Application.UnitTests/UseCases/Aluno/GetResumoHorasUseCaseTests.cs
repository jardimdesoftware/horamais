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

public class GetResumoHorasUseCaseTests
{
    private static void SetPrivate<T, V>(T obj, string prop, V value)
    {
        var p = typeof(T).GetProperty(
            prop,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Public);
        p!.SetValue(obj, value);
    }

    [Fact]
    public async Task Deve_Retornar_Resumo_Das_Horas()
    {
        // Arrange
        var alunoRepo = new Mock<IAlunoRepository>();
        var limiteRepo = new Mock<ILimiteHorasAlunoRepository>();

        var cursoId = Guid.NewGuid();

        var turma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithCursoId(cursoId)
            .WithMaximoHorasExtensao(20)
            .Build();

        var aluno = new AlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("Aluno X")
            .WithEmail("x@ifpe.edu.br")
            .WithMatricula("123")
            .WithTurmaId(turma.Id)
            .WithIdentityUserId("IDX")
            .Build();

        SetPrivate(aluno, nameof(aluno.Turma), turma);

        var ativExt = new DomainAtividade
        {
            Id = Guid.NewGuid(),
            Grupo = "G1",
            Categoria = "C1",
            CategoriaKey = "K1",
            Nome = "Extensão",
            Tipo = TipoAtividade.EXTENSAO,
            CargaMaximaCurso = 50,
            CargaMaximaSemestral = 10
        };

        aluno.Atividades.Add(new AlunoAtividade
        {
            Id = Guid.NewGuid(),
            AlunoId = aluno.Id,
            HorasConcluidas = 5,
            Atividade = ativExt
        });

        alunoRepo.Setup(r => r.GetAllWithAtividadesAsync())
                 .ReturnsAsync(new[] { aluno });

        limiteRepo.Setup(r => r.GetByCursoIdAsync(cursoId))
                  .ReturnsAsync(new LimiteHorasAluno
                  {
                      Id = Guid.NewGuid(),
                      CursoId = cursoId,
                      MaximoHorasComplementar = 10
                  });

        var useCase = new GetResumoHorasUseCase(alunoRepo.Object, limiteRepo.Object);

        // Act
        var result = (await useCase.ExecuteAsync()).ToList();

        // Assert
        result.Should().HaveCount(1);

        var item = result[0];
        item.Nome.Should().Be("Aluno X");

        // 🔧 aqui estavam os nomes errados
        item.TotalHorasExtensao.Should().Be(5);
        item.MaximoHorasExtensao.Should().Be(20);
    }
}
