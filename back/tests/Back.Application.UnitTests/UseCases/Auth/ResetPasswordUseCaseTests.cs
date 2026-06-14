using Back.Application.DTOs.Auth;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Auth;
using Back.Domain.Entities.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Back.Application.UnitTests.UseCases.Auth;

public class ResetPasswordUseCaseTests
{
    private readonly Mock<IIdentityLookupService> _identity = new();
    private readonly Mock<IResetPasswordRepository> _repo = new();
    private readonly Mock<UserManager<IdentityUser>> _userManager;

    public ResetPasswordUseCaseTests()
    {
        _userManager = new Mock<UserManager<IdentityUser>>(
            Mock.Of<IUserStore<IdentityUser>>(), null!, null!, null!, null!, null!, null!, null!, null!
        );
    }

    private ResetPasswordUseCase CreateUseCase()
        => new ResetPasswordUseCase(_identity.Object, _repo.Object, _userManager.Object);

    [Fact]
    public async Task Deve_Falhar_Quando_Codigo_Invalido()
    {
        var user = new IdentityUser { Id = "1", Email = "a@b.com" };

        _identity.Setup(i => i.GetByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        _repo.Setup(r => r.GetByUserAndCodeAsync(user.Id, "000000"))
            .ReturnsAsync((ResetPasswordCode?)null);

        var useCase = CreateUseCase();

        var act = async () => await useCase.ExecuteAsync(new ResetPasswordRequestDto
        {
            Email = user.Email!,
            Code = "000000",
            NewPassword = "123456"
        });

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Deve_Redefinir_Senha_Com_Sucesso()
    {
        var user = new IdentityUser { Id = "1", Email = "a@b.com" };

        var record = new ResetPasswordCode
        {
            IdentityUserId = user.Id,
            Code = "111111",
            IdentityResetToken = "token",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(5),
            Used = false
        };

        _identity.Setup(i => i.GetByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        _repo.Setup(r => r.GetByUserAndCodeAsync(user.Id, record.Code))
            .ReturnsAsync(record);

        _userManager.Setup(u => u.ResetPasswordAsync(user, record.IdentityResetToken, "novaSenha"))
            .ReturnsAsync(IdentityResult.Success);

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(new ResetPasswordRequestDto
        {
            Email = user.Email!,
            Code = record.Code,
            NewPassword = "novaSenha"
        });

        record.Used.Should().BeTrue();
        _repo.Verify(r => r.UpdateAsync(record), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(), Times.Once);

        result.Should().NotBeNull();
    }
}
