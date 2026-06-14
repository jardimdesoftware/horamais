namespace Back.Application.DTOs.Auth;

public record LoginResponseDto(
    string Nome,
    string Email,
    string Role,
    string Token
);
