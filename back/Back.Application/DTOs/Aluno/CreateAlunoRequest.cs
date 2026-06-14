using System;

namespace Back.Application.DTOs.Aluno;

public record CreateAlunoRequest(
    string Nome,
    string Email,
    string Matricula,
    string Senha,
    string? TurmaCodigo = null,
    Guid? TurmaId = null
);
