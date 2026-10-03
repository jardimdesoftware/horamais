using Back.Application.DTOs.Coordenador;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Coordenador;
using Back.Domain.Entities.Convite;
using CoordenadorEntity = Back.Domain.Entities.Coordenador.Coordenador;
using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;

namespace Back.Application.UnitTests.UseCases.Coordenador;

public class CriarCoordenadorUseCaseTests
{
    [Fact]
    public async Task Deve_Criar_Com_Email_Ifpe()
    {
        var conviteRepo = new Mock<IConviteCoordenadorRepository>();
        var coordenadorRepo = new Mock<ICoordenadorRepository>();
        var identity = new Mock<IIdentityService>();

        var token = "token";
        var cursoId = Guid.NewGuid();
        var convite = new ConviteCoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("coordenador@ifpe.edu.br")
            .WithCursoId(cursoId)
            .WithToken(token)
            .WithExpiracao(DateTime.UtcNow.AddHours(1))
            .Build();

        conviteRepo.Setup(r => r.GetValidByTokenAsync(token))
            .ReturnsAsync(convite);
        conviteRepo.Setup(r => r.TryConsumeAsync(convite.Id)).ReturnsAsync(true);
        identity.Setup(i => i.CreateUserAsync("coordenador@ifpe.edu.br", "Senha@123", "COORDENADOR", true))
            .ReturnsAsync((true, "user-id", Array.Empty<string>()));

        var useCase = new CriarCoordenadorUseCase(
            conviteRepo.Object,
            coordenadorRepo.Object,
            identity.Object);

        var request = new CadastroCoordenadorRequest(
            "Nome",
            "123/2025",
            "2025-01-01",
            "coordenador@ifpe.edu.br",
            "Senha@123",
            token);

        var result = await useCase.ExecuteAsync(request);

        result.Email.Should().Be("coordenador@ifpe.edu.br");
        coordenadorRepo.Verify(r => r.AddAsync(It.IsAny<CoordenadorEntity>()), Times.Once);
        conviteRepo.Verify(r => r.TryConsumeAsync(convite.Id), Times.Once);
    }

    [Theory]
    [InlineData("coordenador@discente.ifpe.edu.br")]
    [InlineData("coordenador@docente.ifpe.edu.br")]
    public async Task Deve_Aceitar_Email_Com_Subdominio_Ifpe(string email)
    {
        var conviteRepo = new Mock<IConviteCoordenadorRepository>();
        var coordenadorRepo = new Mock<ICoordenadorRepository>();
        var identity = new Mock<IIdentityService>();

        var token = "token";
        var cursoId = Guid.NewGuid();
        var convite = new ConviteCoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail(email)
            .WithCursoId(cursoId)
            .WithToken(token)
            .WithExpiracao(DateTime.UtcNow.AddHours(1))
            .Build();

        conviteRepo.Setup(r => r.GetValidByTokenAsync(token))
            .ReturnsAsync(convite);
        conviteRepo.Setup(r => r.TryConsumeAsync(convite.Id)).ReturnsAsync(true);
        identity.Setup(i => i.CreateUserAsync(email, "Senha@123", "COORDENADOR", true))
            .ReturnsAsync((true, "user-id", Array.Empty<string>()));

        var useCase = new CriarCoordenadorUseCase(
            conviteRepo.Object,
            coordenadorRepo.Object,
            identity.Object);

        var request = new CadastroCoordenadorRequest(
            "Nome", "123/2025", "2025-01-01", email, "Senha@123", token);

        var result = await useCase.ExecuteAsync(request);

        result.Email.Should().Be(email);
        coordenadorRepo.Verify(r => r.AddAsync(It.IsAny<CoordenadorEntity>()), Times.Once);
    }

    [Fact]
    public async Task Deve_Rejeitar_Email_Docente_Legado()
    {
        var conviteRepo = new Mock<IConviteCoordenadorRepository>();
        var coordenadorRepo = new Mock<ICoordenadorRepository>();
        var identity = new Mock<IIdentityService>();
        var useCase = new CriarCoordenadorUseCase(
            conviteRepo.Object,
            coordenadorRepo.Object,
            identity.Object);

        var request = new CadastroCoordenadorRequest(
            "Nome",
            "123/2025",
            "2025-01-01",
            "coordenador@gmail.com",
            "Senha@123",
            "token");

        Func<Task> act = () => useCase.ExecuteAsync(request);

        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Email institucional inválido*");
    }

    [Fact]
    public async Task Deve_Rejeitar_Email_Diferente_Do_Convite()
    {
        var conviteRepo = new Mock<IConviteCoordenadorRepository>();
        var coordenadorRepo = new Mock<ICoordenadorRepository>();
        var identity = new Mock<IIdentityService>();
        var convite = new ConviteCoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("destinatario@ifpe.edu.br")
            .WithCursoId(Guid.NewGuid())
            .WithToken("token")
            .WithExpiracao(DateTime.UtcNow.AddHours(1))
            .Build();
        conviteRepo.Setup(r => r.GetValidByTokenAsync("token")).ReturnsAsync(convite);

        var useCase = new CriarCoordenadorUseCase(conviteRepo.Object, coordenadorRepo.Object, identity.Object);
        Func<Task> act = () => useCase.ExecuteAsync(new CadastroCoordenadorRequest(
            "Nome", "123/2025", "2025-01-01", "outra@ifpe.edu.br", "Senha@123", "token"));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*não corresponde ao convite*");
        conviteRepo.Verify(r => r.TryConsumeAsync(It.IsAny<Guid>()), Times.Never);
    }
}
