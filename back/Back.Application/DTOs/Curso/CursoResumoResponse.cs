using System;

namespace Back.Application.DTOs.Curso;

public record CursoResumoResponse(
    Guid Id,
    string Nome,
    int QuantidadeTurmas,
    int QuantidadeAlunos,
    Guid CampusId,
    string NomeCampus
);
