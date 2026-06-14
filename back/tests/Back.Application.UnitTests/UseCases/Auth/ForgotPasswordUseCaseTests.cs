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

public class ForgotPasswordUseCaseTests
{
    private readonly Mock<IIdentityLookupService> _identityLookup = new();
    private readonly Mock<IResetPasswordRepository> _repo = new();
    private readonly Mock<UserManager<IdentityUser>> _userManager;
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<IEmailTemplateService> _templateService = new();

    public ForgotPasswordUseCaseTests()
    {
        _userManager = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(),
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            null!
        );
    }

    private ForgotPasswordUseCase CreateUseCase()
        => new ForgotPasswordUseCase(
            _identityLookup.Object,
            _repo.Object,
            _userManager.Object,
            _emailService.Object,
            _templateService.Object
        );

    [Fact]
    public async Task Deve_Retornar_Resposta_Padrao_Quando_Email_Nao_Existe()
    {
        _identityLookup.Setup(x => x.GetByEmailAsync("x@x.com"))
            .ReturnsAsync((IdentityUser?)null);

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(new ForgotPasswordRequestDto { Email = "x@x.com" });

        result.Should().NotBeNull();
        result.Message.Should().NotBeNull();
    }

    [Fact]
    public async Task Deve_Invalidar_Codigo_Anterior()
    {
        var user = new IdentityUser { Id = "abc", Email = "user@ifpe.com" };

        var active = new ResetPasswordCode { IdentityUserId = user.Id, Used = false };

        _identityLookup.Setup(x => x.GetByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        _repo.Setup(x => x.GetActiveByUserAsync(user.Id))
            .ReturnsAsync(active);

        _userManager.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("token123");

        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(new ForgotPasswordRequestDto { Email = user.Email! });

        active.Used.Should().BeTrue();
        _repo.Verify(x => x.UpdateAsync(active), Times.Once);
        _repo.Verify(x => x.SaveChangesAsync(), Times.Exactly(2));
    }

    [Fact]
    public async Task Deve_Criar_Novo_Codigo_E_Enviar_Email()
    {
        var user = new IdentityUser { Id = "abc", Email = "user@ifpe.com" };

        _identityLookup.Setup(x => x.GetByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        _repo.Setup(x => x.GetActiveByUserAsync(user.Id))
            .ReturnsAsync((ResetPasswordCode?)null);

        _userManager.Setup(x => x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("token123");

        var useCase = CreateUseCase();

        await useCase.ExecuteAsync(new ForgotPasswordRequestDto { Email = user.Email! });

        _repo.Verify(r => r.AddAsync(It.IsAny<ResetPasswordCode>()), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.AtLeastOnce);
        _emailService.Verify(
            x => x.EnviarEmailAsync(user.Email!, It.IsAny<string>(), It.IsAny<string>()),
            Times.Once
        );
    }
}
