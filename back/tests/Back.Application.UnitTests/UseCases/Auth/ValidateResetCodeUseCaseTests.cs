using Back.Application.DTOs.Auth;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using Back.Application.UseCases.Auth;
using Back.Domain.Entities.Auth;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Back.Application.UnitTests.UseCases.Auth;

public class ValidateResetCodeUseCaseTests
{
    private readonly Mock<IIdentityLookupService> _identity = new();
    private readonly Mock<IResetPasswordRepository> _repo = new();

    private ValidateResetCodeUseCase CreateUseCase()
        => new ValidateResetCodeUseCase(_identity.Object, _repo.Object);

    [Fact]
    public async Task Deve_Retornar_True_Quando_Email_Inexistente()
    {
        _identity.Setup(x => x.GetByEmailAsync("x@x.com"))
            .ReturnsAsync((IdentityUser?)null);

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(
            new ValidateCodeRequestDto { Email = "x@x.com", Code = "000000" });

        result.Valid.Should().BeTrue();
    }

    [Fact]
    public async Task Deve_Retornar_False_Quando_Codigo_Invalido()
    {
        var user = new IdentityUser { Id = "u1", Email = "a@b.com" };

        _identity.Setup(x => x.GetByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        _repo.Setup(x => x.GetByUserAndCodeAsync(user.Id, "111111"))
            .ReturnsAsync((ResetPasswordCode?)null);

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(
            new ValidateCodeRequestDto { Email = user.Email!, Code = "111111" });

        result.Valid.Should().BeFalse();
    }

    [Fact]
    public async Task Deve_Aceitar_Codigo_Valido()
    {
        var user = new IdentityUser { Id = "u1", Email = "a@b.com" };

        var record = new ResetPasswordCode
        {
            IdentityUserId = user.Id,
            Code = "222222",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(1),
            Used = false
        };

        _identity.Setup(x => x.GetByEmailAsync(user.Email!))
            .ReturnsAsync(user);

        _repo.Setup(x => x.GetByUserAndCodeAsync(user.Id, record.Code))
            .ReturnsAsync(record);

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(
            new ValidateCodeRequestDto { Email = user.Email!, Code = record.Code });

        result.Valid.Should().BeTrue();
        record.Attempts.Should().Be(1);
        _repo.Verify(x => x.UpdateAsync(record), Times.Once);
    }
}
