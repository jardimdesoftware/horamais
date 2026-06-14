using System;
using Back.Domain.Entities.Certificado;

namespace Back.Application.DTOs.Certificado;

public record CertificadoResponse(
    Guid Id,
    string TituloAtividade,
    string Instituicao,
    string Local,
    string Categoria,
    string Grupo,
    string PeriodoLetivo,
    int CargaHoraria,
    DateTime DataInicio,
    DateTime DataFim,
    int TotalPeriodos,
    string? Descricao,
    TipoCertificado Tipo,
    StatusCertificado Status,
    Guid AlunoId,
    Guid AtividadeId,
    string CategoriaKey,
    string? JustificativaRejeicao,
    int? CargaHorariaOriginal,
    bool CargaHorariaCorrigida
);
