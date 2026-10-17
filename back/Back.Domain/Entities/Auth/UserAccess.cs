namespace Back.Domain.Entities.Auth;

/// <summary>Administrative suspension, independent of temporary password lockouts.</summary>
public class UserAccess
{
    public string IdentityUserId { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

/// <summary>Permanent receipt: changing the configured email must not repeat bootstrap.</summary>
public class AdminInitialization
{
    public int Id { get; set; }
    public DateTime CompletedAtUtc { get; set; }
}
