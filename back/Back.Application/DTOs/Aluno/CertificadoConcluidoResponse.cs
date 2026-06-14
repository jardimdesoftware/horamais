using System;

namespace Back.Application.DTOs.Aluno;

public record CertificadoConcluidoResponse(
    Guid Id,
    string? Grupo,
    string? Categoria,
    string? Titulo,
    string? Descricao,
    int CargaHoraria,
    string? Local,
    string? PeriodoInicio,
    string? PeriodoFim,
    string Status,
    string Tipo
);