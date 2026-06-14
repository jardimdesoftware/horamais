namespace Back.Application.DTOs.Auth;

public class ValidateCodeResponseDto
{
    public bool Valid { get; set; }
    public string Message { get; set; } = default!;
}
