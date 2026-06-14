using Back.Domain.Entities.Auth;
using FluentAssertions;

namespace Back.Domain.UnitTests;

public class ResetPasswordCodeTests
{
    [Fact]
    public void ResetPasswordCode_Valido_Deve_Passar()
    {
        var reset = new ResetPasswordCode
        {
            Id = Guid.NewGuid(),
            IdentityUserId = "user-123",
            Code = "123456",
            IdentityResetToken = "token123",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
            Used = false,
            Attempts = 0,
            CreatedAtUtc = DateTime.UtcNow
        };

        var results = ValidationHelper.ValidateObject(reset);

        results.Should().BeEmpty();
    }

    [Fact]
    public void ResetPasswordCode_Sem_Code_Deve_Falhar()
    {
        var reset = new ResetPasswordCode
        {
            Id = Guid.NewGuid(),
            IdentityUserId = "user-123",
            IdentityResetToken = "token123",
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
        };

        var results = ValidationHelper.ValidateObject(reset);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(ResetPasswordCode.Code)));
    }

    [Fact]
    public void ResetPasswordCode_Code_Maior_Que_6_Deve_Falhar()
    {
        var reset = new ResetPasswordCode
        {
            Id = Guid.NewGuid(),
            IdentityUserId = "abc",
            Code = "1234567", // 7 dígitos
            IdentityResetToken = "token",
            ExpiresAtUtc = DateTime.UtcNow
        };

        var results = ValidationHelper.ValidateObject(reset);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(ResetPasswordCode.Code)));
    }
}
