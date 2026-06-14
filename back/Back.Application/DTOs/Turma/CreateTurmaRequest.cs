using System;

namespace Back.Application.DTOs.Turma;

public record CreateTurmaRequest(
    string Periodo,
    string Turno,
    bool PossuiExtensao,
    int? MaximoHorasExtensao,
    Guid CursoId
);
