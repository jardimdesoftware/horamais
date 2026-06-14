using System;

namespace Back.Application.DTOs.Turma;

public record TurmaResponse(
    Guid Id,
    string Periodo,
    string Turno,
    string Codigo,
    bool CodigoAtivo,
    bool PossuiExtensao,
    int? MaximoHorasExtensao,
    Guid CursoId,
    string CursoNome,
    int QuantidadeAlunos
);
