using System;
namespace Back.Application.DTOs.Aluno;
public record AlunoResumoHorasResponse(
    Guid Id,
    string Nome,
    string Email,
    int TotalHorasExtensao,
    int MaximoHorasExtensao,
    int TotalHorasComplementar,
    int MaximoHorasComplementar,
    bool IsAtivo);
