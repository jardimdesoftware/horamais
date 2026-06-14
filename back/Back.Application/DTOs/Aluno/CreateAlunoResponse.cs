using System;

namespace Back.Application.DTOs.Aluno;

public record CreateAlunoResponse(
    Guid Id,
    string Nome,
    string Email
);
