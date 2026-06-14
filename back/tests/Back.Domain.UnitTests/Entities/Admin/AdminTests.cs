using Back.Domain.Entities.Admin;
using FluentAssertions;

namespace Back.Domain.UnitTests;

public class AdminTests
{
    [Fact]
    public void Admin_Valido_Deve_Passar()
    {
        var admin = new AdminBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("admin@teste.com")
            .WithIdentityUserId("abc123")
            .Build();

        var results = ValidationHelper.ValidateObject(admin);

        results.Should().BeEmpty();
    }

    [Fact]
    public void Admin_Sem_Email_Deve_Falhar()
    {
        var admin = new AdminBuilder()
            .WithId(Guid.NewGuid())
            .WithIdentityUserId("abc123")
            .Build();

        var results = ValidationHelper.ValidateObject(admin);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(Admin.Email)));
    }

    [Fact]
    public void Admin_Email_Invalido_Deve_Falhar()
    {
        var admin = new AdminBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail("errado")
            .WithIdentityUserId("abc123")
            .Build();

        var results = ValidationHelper.ValidateObject(admin);

        results.Should().Contain(r => r.MemberNames.Contains(nameof(Admin.Email)));
    }
}
