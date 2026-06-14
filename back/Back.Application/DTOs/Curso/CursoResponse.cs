using System;

namespace Back.Application.DTOs.Curso;

public record CursoResponse(
    Guid Id,
    string Nome
);
