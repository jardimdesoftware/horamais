namespace Back.Application.DTOs.Auth;

public class ForgotPasswordResponseDto
{
    public string Message { get; set; } = "Se existir conta para o e-mail informado, um código foi enviado.";
}
