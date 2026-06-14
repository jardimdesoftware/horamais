using System;

namespace Back.Application.DTOs.Turma;

public record TurmaAlunoResponse(
    Guid Id,
    string Nome,
    string Email,
    string Matricula,
    bool IsAtivo
);
