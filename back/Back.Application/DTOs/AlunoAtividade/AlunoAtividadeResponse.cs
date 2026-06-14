using System;

namespace Back.Application.DTOs.AlunoAtividade;

public record AlunoAtividadeResponse(Guid Id, Guid AlunoId, Guid AtividadeId, int HorasConcluidas);
