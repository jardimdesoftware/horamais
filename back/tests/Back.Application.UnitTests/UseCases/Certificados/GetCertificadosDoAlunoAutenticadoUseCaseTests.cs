using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Certificado;
using Back.Domain.Entities.Aluno;
using Back.Domain.Entities.AlunoAtividade;
using Back.Domain.Entities.Atividade;
using Back.Domain.Entities.Certificado;
using Back.Domain.Entities.Turma;
using FluentAssertions;
using Moq;

namespace Back.Application.UnitTests.UseCases.Certificados;

public class GetCertificadosDoAlunoAutenticadoUseCaseTests
{
    private readonly Mock<ICertificadoRepository> _repo = new();
    private readonly Mock<IAlunoRepository> _alunoRepo = new();

    private GetCertificadosDoAlunoAutenticadoUseCase CreateUseCase()
        => new(_repo.Object, _alunoRepo.Object);

    [Fact]
    public async Task Deve_Retornar_Certificados_Do_Aluno()
    {
        var aluno = new AlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithIdentityUserId("uid")
            .Build();

        _alunoRepo.Setup(r => r.GetByIdentityUserIdAsync("uid"))
            .ReturnsAsync(aluno);

        var cert = new Certificado
        {
            Id = Guid.NewGuid(),
            TituloAtividade = "Cert1",
            Instituicao = "IFPE",
            Local = "Campus",
            Categoria = "C",
            Grupo = "G",
            PeriodoLetivo = "2024.1",
            CargaHoraria = 10,
            DataInicio = DateTime.Today,
            DataFim = DateTime.Today,
            TotalPeriodos = 1,
            Tipo = TipoCertificado.COMPLEMENTAR,
            Status = StatusCertificado.PENDENTE,
            AlunoAtividade = new AlunoAtividade
            {
                AlunoId = aluno.Id,
                AtividadeId = Guid.NewGuid(),
                Atividade = new Back.Domain.Entities.Atividade.Atividade { Id = Guid.NewGuid(), CategoriaKey = "CK" }
            }
        };

        _repo.Setup(r => r.GetAsync(null, aluno.Id))
            .ReturnsAsync(new List<Certificado> { cert });

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync("uid");

        result.Should().HaveCount(1);
        result.First().TituloAtividade.Should().Be("Cert1");
    }
}
