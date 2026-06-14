using System.ComponentModel.DataAnnotations;

namespace Back.Application.DTOs.Auth;

public class ForgotPasswordRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = default!;
}
