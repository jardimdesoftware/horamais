using Back.Application.DTOs.Coordenador;
using Back.Application.Interfaces;
using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Coordenador;
using Back.Domain.Entities.Convite;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.UnitTests.UseCases.Coordenador;

public class EnviarConviteUseCaseTests
{
    [Fact]
    public async Task Deve_Aceitar_Email_Ifpe()
    {
        var conviteRepo = new Mock<IConviteCoordenadorRepository>();
        var emailService = new Mock<IEmailService>();
        var templateService = new Mock<IEmailTemplateService>();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["COORDENADOR_CONVITE_LINK"] = "https://example.com/convite"
            })
            .Build();

        templateService.Setup(t => t.RenderConviteCoordenador(It.IsAny<string>()))
            .Returns("<p>convite</p>");

        var useCase = new EnviarConviteUseCase(
            conviteRepo.Object,
            emailService.Object,
            templateService.Object,
            config);

        var request = new ConviteCoordenadorRequest("coordenador@ifpe.edu.br", Guid.NewGuid());

        await useCase.ExecuteAsync(request);

        emailService.Verify(s => s.EnviarEmailAsync(
            request.Email,
            "Convite para cadastro de Coordenador — hora+",
            It.IsAny<string>()), Times.Once);
        conviteRepo.Verify(r => r.AddAsync(It.IsAny<ConviteCoordenador>()), Times.Once);
        conviteRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData("coordenador@discente.ifpe.edu.br")]
    [InlineData("coordenador@docente.ifpe.edu.br")]
    public async Task Deve_Aceitar_Email_Com_Subdominio_Ifpe(string email)
    {
        var conviteRepo = new Mock<IConviteCoordenadorRepository>();
        var emailService = new Mock<IEmailService>();
        var templateService = new Mock<IEmailTemplateService>();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["COORDENADOR_CONVITE_LINK"] = "https://example.com/convite"
            })
            .Build();

        templateService.Setup(t => t.RenderConviteCoordenador(It.IsAny<string>()))
            .Returns("<p>convite</p>");

        var useCase = new EnviarConviteUseCase(
            conviteRepo.Object,
            emailService.Object,
            templateService.Object,
            config);

        var request = new ConviteCoordenadorRequest(email, Guid.NewGuid());

        await useCase.ExecuteAsync(request);

        emailService.Verify(s => s.EnviarEmailAsync(
            email,
            "Convite para cadastro de Coordenador — hora+",
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Deve_Rejeitar_Email_Docente_Legado()
    {
        var conviteRepo = new Mock<IConviteCoordenadorRepository>();
        var emailService = new Mock<IEmailService>();
        var templateService = new Mock<IEmailTemplateService>();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["COORDENADOR_CONVITE_LINK"] = "https://example.com/convite"
            })
            .Build();

        var useCase = new EnviarConviteUseCase(
            conviteRepo.Object,
            emailService.Object,
            templateService.Object,
            config);

        var request = new ConviteCoordenadorRequest(
            "coordenador@gmail.com",
            Guid.NewGuid());

        Func<Task> act = () => useCase.ExecuteAsync(request);

        await act.Should()
            .ThrowAsync<ArgumentException>()
            .WithMessage("Email institucional inválido*");
    }
}
