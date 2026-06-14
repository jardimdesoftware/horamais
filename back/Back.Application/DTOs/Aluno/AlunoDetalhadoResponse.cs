using System.Collections.Generic;
using System;

namespace Back.Application.DTOs.Aluno;
public record AlunoDetalhadoResponse(
    Guid Id,
    string Nome,
    string Email,
    string Matricula,
    bool IsAtivo,
    Guid TurmaId,
    IEnumerable<AtividadeAlunoResumo> Atividades,
    int TotalHorasExtensao,
    int MaximoHorasExtensao,
    int TotalHorasComplementar,
    int MaximoHorasComplementar
);