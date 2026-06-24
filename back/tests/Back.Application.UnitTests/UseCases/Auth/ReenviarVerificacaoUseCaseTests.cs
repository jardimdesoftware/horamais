using Back.Application.DTOs.Auth;
using Back.Application.Interfaces;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Auth;
using Back.Domain.Entities.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Back.Application.UnitTests.UseCases.Auth;

public class ReenviarVerificacaoUseCaseTests
{
    private readonly Mock<IIdentityLookupService> _identityLookup = new();
    private readonly Mock<IEmailVerificationRepository> _repo = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IEmailTemplateService> _templateService = new();

    private ReenviarVerificacaoUseCase CreateUseCase()
        => new ReenviarVerificacaoUseCase(
            _identityLookup.Object,
            _repo.Object,
            _emailService.Object,
            _templateService.Object
        );

    [Fact]
    public async Task Deve_Ser_Neutro_Quando_Email_Nao_Existe()
    {
        _identityLookup.Setup(x => x.GetByEmailAsync("x@x.com"))
            .ReturnsAsync((IdentityUser?)null);

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(new ResendVerificationRequestDto { Email = "x@x.com" });

        result.Should().NotBeNull();
        _repo.Verify(r => r.AddAsync(It.IsAny<EmailVerificationCode>()), Times.Never);
        _emailService.Verify(
            x => x.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Deve_Ser_Neutro_Quando_Ja_Confirmado()
    {
        var user = new IdentityUser { Id = "1", Email = "a@b.com", EmailConfirmed = true };

        _identityLookup.Setup(x => x.GetByEmailAsync(user.Email!)).ReturnsAsync(user);

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(new ResendVerificationRequestDto { Email = user.Email! });

        result.Should().NotBeNull();
        _repo.Verify(r => r.AddAsync(It.IsAny<EmailVerificationCode>()), Times.Never);
        _emailService.Verify(
            x => x.EnviarEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task Deve_Gerar_Novo_Codigo_E_Reenviar()
    {
        var user = new IdentityUser { Id = "1", Email = "a@b.com", EmailConfirmed = false };

        _identityLookup.Setup(x => x.GetByEmailAsync(user.Email!)).ReturnsAsync(user);
        _repo.Setup(x => x.GetActiveByUserAsync(user.Id))
            .ReturnsAsync((EmailVerificationCode?)null);

        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(new ResendVerificationRequestDto { Email = user.Email! });

        _repo.Verify(r => r.AddAsync(It.Is<EmailVerificationCode>(
            c => c.IdentityUserId == user.Id && c.Code.Length == 6)), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        _emailService.Verify(
            x => x.EnviarEmailAsync(user.Email!, It.IsAny<string>(), It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task Deve_Invalidar_Codigo_Ativo_Anterior()
    {
        var user = new IdentityUser { Id = "1", Email = "a@b.com", EmailConfirmed = false };
        var active = new EmailVerificationCode { IdentityUserId = user.Id, Code = "111111", Used = false };

        _identityLookup.Setup(x => x.GetByEmailAsync(user.Email!)).ReturnsAsync(user);
        _repo.Setup(x => x.GetActiveByUserAsync(user.Id)).ReturnsAsync(active);

        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(new ResendVerificationRequestDto { Email = user.Email! });

        active.Used.Should().BeTrue();
        _repo.Verify(r => r.UpdateAsync(active), Times.Once);
        _repo.Verify(r => r.AddAsync(It.IsAny<EmailVerificationCode>()), Times.Once);
    }
}
