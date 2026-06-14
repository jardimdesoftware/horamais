using Back.Domain.Entities.AlunoAtividade;
using Back.Domain.Entities.Atividade;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.Interfaces.Repositories;

public interface IAlunoAtividadeRepository
{
    Task AddRangeAsync(IEnumerable<AlunoAtividade> alunoAtividades);
    Task<AlunoAtividade?> GetByAlunoEAtividadeAsync(Guid alunoId, Guid atividadeId);
    Task UpdateAsync(AlunoAtividade alunoAtividade);
    Task<int> GetTotalHorasConcluidasPorTipoAsync(Guid alunoId, TipoAtividade tipo);
    Task<IEnumerable<AlunoAtividade>> GetByAtividadeIdAsync(Guid atividadeId);
    Task RemoveRangeAsync(IEnumerable<AlunoAtividade> entidades);
    Task<IEnumerable<AlunoAtividade>> GetByAlunoIdTrackedAsync(Guid alunoId);
}
