using System.ComponentModel.DataAnnotations;

namespace Back.Application.DTOs.Auth;

public class ValidateCodeRequestDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = default!;

    [Required, MinLength(6), MaxLength(6)]
    public string Code { get; set; } = default!;
}
