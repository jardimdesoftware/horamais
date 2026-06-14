using System;

namespace Back.Application.DTOs.Turma;

public record VerificarTurmaResponse(
    string Periodo,
    string CursoNome
);
