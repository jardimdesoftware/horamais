using Back.Application.DTOs.Aluno;
using Back.Application.Interfaces;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using Back.Application.Interfaces.Services;
using Back.Application.UseCases.Aluno;
using Back.Domain.Entities.Auth;
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
    private readonly Mock<ITurmaRealtimeNotifier> _realtime = new();
    private readonly Mock<IEmailVerificationRepository> _verificationRepo = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IEmailTemplateService> _templateService = new();

    private CreateAlunoUseCase CreateUseCase()
        => new CreateAlunoUseCase(
            _alunoRepo.Object,
            _turmaRepo.Object,
            _atividadeRepo.Object,
            _alunoAtividadeRepo.Object,
            _identityService.Object,
            _realtime.Object,
            _verificationRepo.Object,
            _emailService.Object,
            _templateService.Object
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

        _identityService.Setup(r => r.CreateUserAsync("aluno@ifpe.edu.br", "123", "ALUNO", false))
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

        // Conta criada pendente: código de verificação persistido e e-mail enviado.
        _verificationRepo.Verify(r => r.AddAsync(It.Is<EmailVerificationCode>(
            c => c.IdentityUserId == "identity-1" && c.Code.Length == 6)), Times.Once);
        _verificationRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _emailService.Verify(s => s.EnviarEmailAsync(
            "aluno@ifpe.edu.br", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
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

        _identityService.Setup(r => r.CreateUserAsync(email, "123", "ALUNO", false))
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
