using Back.Application.UseCases.Certificado;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Certificado;
using Back.Domain.Entities.AlunoAtividade;
using Back.Domain.Entities.Atividade;
using Back.Domain.Entities.Aluno;
using Back.Domain.Entities.Turma;
using FluentAssertions;
using Moq;
using DomainAluno = Back.Domain.Entities.Aluno.Aluno;

namespace Back.Application.UnitTests.UseCases.Certificados;

public class GetCertificadosByCursoIdUseCaseTests
{
    private readonly Mock<ICertificadoRepository> _repo = new();

    private GetCertificadosByCursoIdUseCase CreateUseCase()
        => new(_repo.Object);

    private static void SetPrivateProp<T, P>(T obj, string prop, P value)
    {
        typeof(T).GetProperty(prop,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic)!
        .SetValue(obj, value);
    }

    [Fact]
    public async Task Deve_Listar_Certificados_Do_Curso()
    {
        // Arrange
        var cursoId = Guid.NewGuid();

        var turma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithPeriodo("2024.1")
            .WithTurno("Noite")
            .WithCursoId(cursoId)
            .Build();

        var aluno = new AlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("Aluno Teste")
            .WithEmail("a@ifpe.edu.br")
            .WithMatricula("001")
            .WithTurmaId(turma.Id)
            .Build();

        // 🚀 IMPORTANTE: Setar navegação da turma usando reflexão
        SetPrivateProp(aluno, nameof(DomainAluno.Turma), turma);

        var atividade = new Back.Domain.Entities.Atividade.Atividade
        {
            Id = Guid.NewGuid(),
            Nome = "Atividade Teste",
            Categoria = "CAT",
            CategoriaKey = "CATKEY",
            Grupo = "G1",
            CargaMaximaCurso = 80,
            CargaMaximaSemestral = 40,
            Tipo = TipoAtividade.COMPLEMENTAR
        };

        var alunoAtividade = new AlunoAtividade
        {
            Id = Guid.NewGuid(),
            AlunoId = aluno.Id,
            Aluno = aluno,
            AtividadeId = atividade.Id,
            Atividade = atividade,
            HorasConcluidas = 0
        };

        var cert = new Certificado
        {
            Id = Guid.NewGuid(),
            Grupo = "G1",
            Categoria = "C1",
            TituloAtividade = "Certificado Teste",
            CargaHoraria = 10,
            Local = "IFPE",
            Instituicao = "IFPE",
            PeriodoLetivo = "2024.1",
            DataInicio = DateTime.Today,
            DataFim = DateTime.Today,
            TotalPeriodos = 1,
            Status = StatusCertificado.PENDENTE,
            Tipo = TipoCertificado.COMPLEMENTAR,
            AnexoStorageKey = "certificados/test.pdf",
            AlunoAtividadeId = alunoAtividade.Id,
            AlunoAtividade = alunoAtividade
        };

        _repo.Setup(r => r.GetAllWithAlunoAtividadeAsync())
            .ReturnsAsync(new List<Certificado> { cert });

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.ExecuteAsync(cursoId);

        // Assert
        result.Should().HaveCount(1);

        var item = result.First();
        item.Id.Should().Be(cert.Id);
        item.AlunoId.Should().Be(aluno.Id);
        item.AlunoEmail.Should().Be(aluno.Email);
        item.Categoria.Should().Be(cert.Categoria);
        item.Grupo.Should().Be(cert.Grupo);
        item.TituloAtividade.Should().Be(cert.TituloAtividade);
        item.CargaHoraria.Should().Be(cert.CargaHoraria);
        item.PeriodoTurma.Should().Be(turma.Periodo);
    }
}
