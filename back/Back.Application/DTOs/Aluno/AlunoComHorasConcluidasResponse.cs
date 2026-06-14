using System;
using System.Collections.Generic;

namespace Back.Application.DTOs.Aluno;

public record AlunoComHorasConcluidasResponse(
    Guid Id,
    string? Nome,
    string? Matricula,
    string? Email,
    string? Curso,
    List<CertificadoConcluidoResponse> Certificados,
    int CargaHoraria,
    bool CargaHorariaFinalizada,
    bool JaFezDownload,
    string Categoria
);