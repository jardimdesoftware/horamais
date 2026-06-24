using Back.Application.DTOs.Auth;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Auth;
using Back.Domain.Entities.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Back.Application.UnitTests.UseCases.Auth;

public class ConfirmarEmailUseCaseTests
{
    private readonly Mock<IIdentityLookupService> _identity = new();
    private readonly Mock<IEmailVerificationRepository> _repo = new();
    private readonly Mock<UserManager<IdentityUser>> _userManager;

    public ConfirmarEmailUseCaseTests()
    {
        _userManager = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(), null!, null!, null!, null!, null!, null!, null!, null!
        );
    }

    private ConfirmarEmailUseCase CreateUseCase()
        => new ConfirmarEmailUseCase(_identity.Object, _repo.Object, _userManager.Object);

    [Fact]
    public async Task Deve_Falhar_Quando_Codigo_Invalido()
    {
        var user = new IdentityUser { Id = "1", Email = "a@b.com", EmailConfirmed = false };

        _identity.Setup(i => i.GetByEmailAsync(user.Email!)).ReturnsAsync(user);
        _repo.Setup(r => r.GetByUserAndCodeAsync(user.Id, "000000"))
            .ReturnsAsync((EmailVerificationCode?)null);

        var useCase = CreateUseCase();

        var act = async () => await useCase.ExecuteAsync(new ConfirmEmailRequestDto
        {
            Email = user.Email!,
            Code = "000000"
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Deve_Falhar_Quando_Codigo_Expirado()
    {
        var user = new IdentityUser { Id = "1", Email = "a@b.com", EmailConfirmed = false };
        var record = new EmailVerificationCode
        {
            IdentityUserId = user.Id,
            Code = "111111",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(-1),
            Used = false
        };

        _identity.Setup(i => i.GetByEmailAsync(user.Email!)).ReturnsAsync(user);
        _repo.Setup(r => r.GetByUserAndCodeAsync(user.Id, record.Code)).ReturnsAsync(record);

        var useCase = CreateUseCase();

        var act = async () => await useCase.ExecuteAsync(new ConfirmEmailRequestDto
        {
            Email = user.Email!,
            Code = record.Code
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Deve_Confirmar_Email_Com_Sucesso()
    {
        var user = new IdentityUser { Id = "1", Email = "a@b.com", EmailConfirmed = false };
        var record = new EmailVerificationCode
        {
            IdentityUserId = user.Id,
            Code = "222222",
            ExpiresAtUtc = DateTime.UtcNow.AddHours(1),
            Used = false
        };

        _identity.Setup(i => i.GetByEmailAsync(user.Email!)).ReturnsAsync(user);
        _repo.Setup(r => r.GetByUserAndCodeAsync(user.Id, record.Code)).ReturnsAsync(record);
        _userManager.Setup(u => u.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(new ConfirmEmailRequestDto
        {
            Email = user.Email!,
            Code = record.Code
        });

        user.EmailConfirmed.Should().BeTrue();
        record.Used.Should().BeTrue();
        _repo.Verify(r => r.UpdateAsync(record), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Deve_Ser_Idempotente_Quando_Ja_Confirmado()
    {
        var user = new IdentityUser { Id = "1", Email = "a@b.com", EmailConfirmed = true };

        _identity.Setup(i => i.GetByEmailAsync(user.Email!)).ReturnsAsync(user);

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(new ConfirmEmailRequestDto
        {
            Email = user.Email!,
            Code = "123456"
        });

        result.Should().NotBeNull();
        _repo.Verify(r => r.GetByUserAndCodeAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _userManager.Verify(u => u.UpdateAsync(It.IsAny<IdentityUser>()), Times.Never);
    }
}
