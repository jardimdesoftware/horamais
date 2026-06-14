using System;

namespace Back.Application.DTOs.Aluno;

public record AlunoEmRiscoResponse(
    Guid Id,
    string Nome,
    string Matricula,
    string TurmaPeriodo,
    string TurmaCodigo,
    string CursoNome,
    int TotalHorasComplementar,
    int MaximoHorasComplementar,
    double PorcentagemConclusao,
    int PeriodosDecorridos,
    double HorasPorPeriodo
);
