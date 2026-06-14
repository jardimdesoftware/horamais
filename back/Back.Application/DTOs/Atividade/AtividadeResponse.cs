using System;

namespace Back.Application.DTOs.Atividade;

public record AtividadeResponse(Guid Id, string Nome, string Tipo, string Grupo, string Categoria, string CategoriaKey, int CargaMaximaSemestral, int CargaMaximaCurso, bool PossuiCurricularizacaoExtensao, int? HorasCurricularizacaoExtensao);
