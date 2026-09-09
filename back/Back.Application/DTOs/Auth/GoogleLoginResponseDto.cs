using System;

namespace Back.Application.DTOs.Auth;

public record GoogleLoginResponseDto(
    string Nome,
    string Email,
    string? Role,
    string? Token,
    bool RequiresRegistration = false)
{
    public static GoogleLoginResponseDto ForFirstAccess(string nome, string email)
    {
        var parts = email.Split('@');
        if (parts.Length != 2 || string.IsNullOrWhiteSpace(parts[0]) ||
            !string.Equals(parts[1], "discente.ifpe.edu.br", StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException("O primeiro acesso com Google exige um e-mail @discente.ifpe.edu.br.");

        // Primeiro acesso não concede perfil nem token de acesso à aplicação.
        return new(nome, email, null, null, true);
    }
}
