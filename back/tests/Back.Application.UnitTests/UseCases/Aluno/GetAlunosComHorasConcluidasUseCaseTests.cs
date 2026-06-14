using System.Security.Claims;
using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Aluno;
using Back.Domain.Entities.Aluno;
using Back.Domain.Entities.AlunoAtividade;
using Back.Domain.Entities.Atividade;
using Back.Domain.Entities.Certificado;
using Back.Domain.Entities.LimiteHorasAluno;
using Back.Domain.Entities.Turma;
using FluentAssertions;
using Moq;
using DomainAtividade = Back.Domain.Entities.Atividade.Atividade;

namespace Back.Application.UnitTests.UseCases.Aluno;

public class GetAlunosComHorasConcluidasUseCaseTests
{
    private ClaimsPrincipal CreateUser(string id)
        => new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, id) }));

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
    public async Task Deve_Retornar_Alunos_Que_Concluíram_Horas()
    {
        var alunoRepo = new Mock<IAlunoRepository>();
        var limiteRepo = new Mock<ILimiteHorasAlunoRepository>();
        var certRepo = new Mock<ICertificadoRepository>();
        var coordenadorRepo = new Mock<ICoordenadorRepository>();

        var coordId = "coord-1";
        var cursoId = Guid.NewGuid();

        var coordenador = new Back.Domain.Entities.Coordenador.CoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("Coord")
            .WithEmail("coord@ifpe.edu.br")
            .WithNumeroPortaria("123")
            .WithDOU("dou")
            .WithCursoId(cursoId)
            .WithIdentityUserId(coordId)
            .Build();

        coordenadorRepo
            .Setup(r => r.GetByIdentityUserIdWithCursoAsync(coordId))
            .ReturnsAsync(coordenador);

        // ----------------------------------------------
        // ALUNO 1 — CONCLUIU EXTENSÃO
        // ----------------------------------------------

        var turma = new TurmaBuilder().WithId(Guid.NewGuid()).WithCursoId(cursoId).WithMaximoHorasExtensao(10).Build();
        var aluno1 = new AlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("Aluno 1")
            .WithEmail("a1@ifpe.edu.br")
            .WithTurmaId(turma.Id)
            .WithMatricula("1")
            .WithIdentityUserId("A1")
            .Build();

        SetPrivate(aluno1, nameof(aluno1.Turma), turma);

        var ativ = new DomainAtividade
        {
            Id = Guid.NewGuid(),
            Nome = "Ativ",
            Grupo = "G",
            Categoria = "C",
            CategoriaKey = "K",
            Tipo = TipoAtividade.EXTENSAO,
            CargaMaximaCurso = 50,
            CargaMaximaSemestral = 10
        };

        aluno1.Atividades.Add(new AlunoAtividade
        {
            Id = Guid.NewGuid(),
            AlunoId = aluno1.Id,
            AtividadeId = ativ.Id,
            HorasConcluidas = 20,
            Atividade = ativ
        });

        certRepo.Setup(r => r.GetCertificadosAprovadosPorAlunoAsync(aluno1.Id))
                .ReturnsAsync(new List<Certificado>());

        alunoRepo.Setup(r => r.GetAlunosPorCursoComDetalhesAsync(cursoId))
                 .ReturnsAsync(new[] { aluno1 });

        limiteRepo.Setup(r => r.GetByCursoIdAsync(cursoId))
                  .ReturnsAsync(new LimiteHorasAluno
                  {
                      Id = Guid.NewGuid(),
                      CursoId = cursoId
                  });

        var useCase = new GetAlunosComHorasConcluidasUseCase(
            alunoRepo.Object, limiteRepo.Object, certRepo.Object, coordenadorRepo.Object
        );

        var result = await useCase.ExecuteAsync(TipoAtividade.EXTENSAO, CreateUser(coordId));

        result.Should().HaveCount(1);
        var alunoRet = result.First();
        alunoRet.Nome.Should().Be("Aluno 1");
        alunoRet.CargaHoraria.Should().Be(20);
        alunoRet.CargaHorariaFinalizada.Should().BeTrue();
    }
}
