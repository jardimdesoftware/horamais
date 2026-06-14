using Back.Application.DTOs.Aluno;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Aluno;
using Back.Domain.Entities.Turma;
using FluentAssertions;
using Moq;

using DomainAtividade = Back.Domain.Entities.Atividade.Atividade;

namespace Back.Application.UnitTests.UseCases.Aluno;

public class CreateAlunoUseCaseTests
{
    private readonly Mock<IAlunoRepository> _alunoRepo = new();
    private readonly Mock<ITurmaRepository> _turmaRepo = new();
    private readonly Mock<IAtividadeRepository> _atividadeRepo = new();
    private readonly Mock<IAlunoAtividadeRepository> _alunoAtividadeRepo = new();
    private readonly Mock<IIdentityService> _identityService = new();

    private CreateAlunoUseCase CreateUseCase()
        => new CreateAlunoUseCase(
            _alunoRepo.Object,
            _turmaRepo.Object,
            _atividadeRepo.Object,
            _alunoAtividadeRepo.Object,
            _identityService.Object
        );

    [Fact]
    public async Task Deve_Criar_Aluno_Com_Sucesso()
    {
        // Arrange
        var turma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithPeriodo("2024.1")
            .WithTurno("Noite")
            .WithCursoId(Guid.NewGuid())
            .Build();

        _turmaRepo.Setup(r => r.GetByIdAsync(turma.Id))
            .ReturnsAsync(turma);

        _identityService.Setup(r => r.CreateUserAsync("aluno@ifpe.edu.br", "123", "ALUNO"))
            .ReturnsAsync((true, "identity-1", Array.Empty<string>()));

        _atividadeRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<DomainAtividade>());

        var request = new CreateAlunoRequest(
            Nome: "Aluno",
            Email: "aluno@ifpe.edu.br",
            Matricula: "0001",
            Senha: "123",
            TurmaId: turma.Id
        );

        var useCase = CreateUseCase();

        // Act
        var result = await useCase.ExecuteAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("aluno@ifpe.edu.br");

        _alunoRepo.Verify(r => r.AddAsync(It.IsAny<Back.Domain.Entities.Aluno.Aluno>()), Times.Once);
    }

    [Theory]
    [InlineData("aluno@discente.ifpe.edu.br")]
    [InlineData("aluno@docente.ifpe.edu.br")]
    public async Task Deve_Aceitar_Email_Com_Subdominio_Ifpe(string email)
    {
        var turma = new TurmaBuilder()
            .WithId(Guid.NewGuid())
            .WithPeriodo("2024.1")
            .WithTurno("Noite")
            .WithCursoId(Guid.NewGuid())
            .Build();

        _turmaRepo.Setup(r => r.GetByIdAsync(turma.Id))
            .ReturnsAsync(turma);

        _identityService.Setup(r => r.CreateUserAsync(email, "123", "ALUNO"))
            .ReturnsAsync((true, "identity-1", Array.Empty<string>()));

        _atividadeRepo.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<DomainAtividade>());

        var request = new CreateAlunoRequest(
            Nome: "Aluno",
            Email: email,
            Matricula: "0001",
            Senha: "123",
            TurmaId: turma.Id
        );

        var useCase = CreateUseCase();
        var result = await useCase.ExecuteAsync(request);

        result.Email.Should().Be(email);
    }

    [Fact]
    public async Task Deve_Rejeitar_Email_Nao_Institucional()
    {
        var request = new CreateAlunoRequest(
            Nome: "Aluno",
            Email: "aluno@gmail.com",
            Matricula: "0001",
            Senha: "123",
            TurmaId: Guid.NewGuid()
        );

        var useCase = CreateUseCase();

        Func<Task> act = () => useCase.ExecuteAsync(request);

        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Email institucional inválido.");
    }
}
