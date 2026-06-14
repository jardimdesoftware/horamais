namespace Back.Application.DTOs.Coordenador;

public record CadastroCoordenadorRequest(
    string Nome,
    string NumeroPortaria,
    string DOU,
    string Email,
    string Senha,
    string Token
);
