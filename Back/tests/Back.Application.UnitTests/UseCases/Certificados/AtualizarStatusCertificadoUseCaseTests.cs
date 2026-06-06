using Back.Application.UseCases.Certificado;
using Back.Application.Interfaces;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Certificado;
using Back.Domain.Entities.Atividade;
using Back.Domain.Entities.AlunoAtividade;
using Back.Domain.Entities.Aluno;
using Back.Domain.Entities.Turma;
using Moq;
using FluentAssertions;

namespace Back.Application.UnitTests.UseCases.Certificados;

public class AtualizarStatusCertificadoUseCaseTests
{
    private readonly Mock<ICertificadoRepository> _certRepo = new();
    private readonly Mock<IAlunoAtividadeRepository> _alunoAtvRepo = new();
    private readonly Mock<ILimiteHorasAlunoRepository> _limiteRepo = new();
    private readonly Mock<INotificarSecretariaConclusaoUseCase> _notificarSecretaria = new();

    private AtualizarStatusCertificadoUseCase CreateUseCase()
        => new(_certRepo.Object, _alunoAtvRepo.Object, _limiteRepo.Object, _notificarSecretaria.Object);

    private static void SetPrivateProp<T, P>(T obj, string prop, P value)
    {
        typeof(T).GetProperty(prop,
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic)!
        .SetValue(obj, value);
    }

    [Fact]
    public async Task Deve_Aprovar_Certificado_Sem_Acrescentar_Horas_Quando_Limite_Zerado()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cursoId = Guid.NewGuid();
        var alunoId = Guid.NewGuid();

        var turma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithPeriodo("2024.1")
            .WithTurno("Noite")
            .WithCursoId(cursoId)
            .Build();

        var aluno = new AlunoBuilder()
            .WithId(alunoId)
            .WithNome("Aluno Teste")
            .WithEmail("aluno@ifpe.edu.br")
            .WithMatricula("2023001")
            .WithTurmaId(turma.Id)
            .WithIdentityUserId("uid")
            .Build();

        // Injeta Turma no Aluno via reflection (private set)
        SetPrivateProp(aluno, nameof(aluno.Turma), turma);

        var atividade = new Back.Domain.Entities.Atividade.Atividade
        {
            Id = Guid.NewGuid(),
            CargaMaximaCurso = 40,
            CargaMaximaSemestral = 10,
            Tipo = TipoAtividade.COMPLEMENTAR
        };

        var alunoAtv = new AlunoAtividade
        {
            Id = Guid.NewGuid(),
            AlunoId = alunoId,
            Aluno = aluno,
            AtividadeId = atividade.Id,
            Atividade = atividade,
            HorasConcluidas = 40
        };

        var cert = new Certificado
        {
            Id = id,
            AlunoAtividadeId = alunoAtv.Id,
            AlunoAtividade = alunoAtv,
            Grupo = "G1",
            PeriodoLetivo = "2024.1",
            CargaHoraria = 10,
            TotalPeriodos = 1,
            Status = StatusCertificado.PENDENTE
        };

        _certRepo.Setup(r => r.GetByIdWithAlunoAtividadeAsync(id))
            .ReturnsAsync(cert);

        _limiteRepo.Setup(r => r.GetByCursoIdAsync(cursoId))
            .ReturnsAsync(new Back.Domain.Entities.LimiteHorasAluno.LimiteHorasAluno
            {
                CursoId = cursoId,
                MaximoHorasComplementar = 40
            });

        _alunoAtvRepo.Setup(r => r.GetTotalHorasConcluidasPorTipoAsync(alunoAtv.AlunoId, TipoAtividade.COMPLEMENTAR))
            .ReturnsAsync(40);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.ExecuteAsync(id, StatusCertificado.APROVADO);

        // Assert
        result.Should().BeTrue();
        alunoAtv.HorasConcluidas.Should().Be(40);

        _certRepo.Verify(r => r.UpdateAsync(It.IsAny<Certificado>()), Times.Once);
    }

    [Fact]
    public async Task Deve_Aprovar_Certificado_Com_Correcao_De_CargaHoraria()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cursoId = Guid.NewGuid();
        var alunoId = Guid.NewGuid();

        var turma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithPeriodo("2024.1")
            .WithTurno("Noite")
            .WithCursoId(cursoId)
            .Build();

        var aluno = new AlunoBuilder()
            .WithId(alunoId)
            .WithNome("Aluno Teste")
            .WithEmail("aluno@ifpe.edu.br")
            .WithMatricula("2023002")
            .WithTurmaId(turma.Id)
            .WithIdentityUserId("uid")
            .Build();

        SetPrivateProp(aluno, nameof(aluno.Turma), turma);

        var atividade = new Back.Domain.Entities.Atividade.Atividade
        {
            Id = Guid.NewGuid(),
            CargaMaximaCurso = 100,
            CargaMaximaSemestral = 100,
            Tipo = TipoAtividade.COMPLEMENTAR
        };

        var alunoAtv = new AlunoAtividade
        {
            Id = Guid.NewGuid(),
            AlunoId = alunoId,
            Aluno = aluno,
            AtividadeId = atividade.Id,
            Atividade = atividade,
            HorasConcluidas = 0
        };

        var cert = new Certificado
        {
            Id = id,
            AlunoAtividadeId = alunoAtv.Id,
            AlunoAtividade = alunoAtv,
            Grupo = "G1",
            PeriodoLetivo = "2024.1",
            CargaHoraria = 20,
            TotalPeriodos = 1,
            Status = StatusCertificado.PENDENTE
        };

        _certRepo.Setup(r => r.GetByIdWithAlunoAtividadeAsync(id))
            .ReturnsAsync(cert);

        _certRepo.Setup(r => r.GetByAlunoAtividadeAsync(cert.AlunoAtividadeId))
            .ReturnsAsync(new List<Certificado>());

        _limiteRepo.Setup(r => r.GetByCursoIdAsync(cursoId))
            .ReturnsAsync(new Back.Domain.Entities.LimiteHorasAluno.LimiteHorasAluno
            {
                CursoId = cursoId,
                MaximoHorasComplementar = 100
            });

        _alunoAtvRepo.Setup(r => r.GetTotalHorasConcluidasPorTipoAsync(alunoAtv.AlunoId, TipoAtividade.COMPLEMENTAR))
            .ReturnsAsync(0);

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.ExecuteAsync(id, StatusCertificado.APROVADO, novaCargaHoraria: 5);

        // Assert
        result.Should().BeTrue();
        cert.CargaHorariaOriginal.Should().Be(20);
        cert.CargaHoraria.Should().Be(5);
        cert.CargaHorariaCorrigida.Should().BeTrue();

        _alunoAtvRepo.Verify(r => r.UpdateAsync(It.IsAny<AlunoAtividade>()), Times.Once);
        _certRepo.Verify(r => r.UpdateAsync(It.IsAny<Certificado>()), Times.Once);
    }

    [Fact]
    public async Task Nao_Deve_Corrigir_CargaHoraria_Quando_Certificado_Ja_Esta_Aprovado()
    {
        // Arrange
        var id = Guid.NewGuid();
        var cursoId = Guid.NewGuid();
        var alunoId = Guid.NewGuid();

        var turma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithPeriodo("2024.1")
            .WithTurno("Noite")
            .WithCursoId(cursoId)
            .Build();

        var aluno = new AlunoBuilder()
            .WithId(alunoId)
            .WithNome("Aluno Teste")
            .WithEmail("aluno@ifpe.edu.br")
            .WithMatricula("2023003")
            .WithTurmaId(turma.Id)
            .WithIdentityUserId("uid")
            .Build();

        SetPrivateProp(aluno, nameof(aluno.Turma), turma);

        var atividade = new Back.Domain.Entities.Atividade.Atividade
        {
            Id = Guid.NewGuid(),
            CargaMaximaCurso = 100,
            CargaMaximaSemestral = 100,
            Tipo = TipoAtividade.COMPLEMENTAR
        };

        var alunoAtv = new AlunoAtividade
        {
            Id = Guid.NewGuid(),
            AlunoId = alunoId,
            Aluno = aluno,
            AtividadeId = atividade.Id,
            Atividade = atividade,
            HorasConcluidas = 0
        };

        var cert = new Certificado
        {
            Id = id,
            AlunoAtividadeId = alunoAtv.Id,
            AlunoAtividade = alunoAtv,
            Grupo = "G1",
            PeriodoLetivo = "2024.1",
            CargaHoraria = 10,
            TotalPeriodos = 1,
            Status = StatusCertificado.APROVADO,
            CargaHorariaCorrigida = false
        };

        _certRepo.Setup(r => r.GetByIdWithAlunoAtividadeAsync(id))
            .ReturnsAsync(cert);

        _limiteRepo.Setup(r => r.GetByCursoIdAsync(cursoId))
            .ReturnsAsync(new Back.Domain.Entities.LimiteHorasAluno.LimiteHorasAluno
            {
                CursoId = cursoId,
                MaximoHorasComplementar = 100
            });

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.ExecuteAsync(id, StatusCertificado.APROVADO, novaCargaHoraria: 5);

        // Assert
        result.Should().BeTrue();
        cert.CargaHoraria.Should().Be(10);
        cert.CargaHorariaOriginal.Should().BeNull();
        cert.CargaHorariaCorrigida.Should().BeFalse();

        _alunoAtvRepo.Verify(r => r.UpdateAsync(It.IsAny<AlunoAtividade>()), Times.Never);
        _certRepo.Verify(r => r.UpdateAsync(It.IsAny<Certificado>()), Times.Once);
    }
}
