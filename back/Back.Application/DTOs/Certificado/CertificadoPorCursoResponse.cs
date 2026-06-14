using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Back.Application.DTOs.Certificado;
public record CertificadoPorCursoResponse(
    Guid Id,
    string Grupo,
    string Categoria,
    string TituloAtividade,
    int CargaHoraria,
    string Local,
    DateTime DataInicio,
    DateTime DataFim,
    string Status,
    string Tipo,
    Guid AlunoId,
    string AlunoNome,
    string AlunoEmail,
    string AlunoMatricula,
    string PeriodoTurma,
    string? JustificativaRejeicao,
    int? CargaHorariaOriginal,
    bool CargaHorariaCorrigida
);
