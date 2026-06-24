namespace Back.Application.DTOs.Auth;

public class ResendVerificationResponseDto
{
    public string Message { get; set; } = "Se houver um cadastro pendente para este e-mail, um novo código foi enviado.";
}
