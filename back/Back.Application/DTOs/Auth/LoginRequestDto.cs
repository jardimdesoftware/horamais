namespace Back.Application.DTOs.Auth;

public record LoginRequestDto(
    string Email,
    string Senha
);
