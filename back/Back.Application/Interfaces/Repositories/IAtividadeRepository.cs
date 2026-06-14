using Back.Domain.Entities.Atividade;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.Interfaces.Repositories;

public interface IAtividadeRepository
{
    Task<List<Atividade>> GetAllAsync();
    Task<List<Atividade>> GetAllTrackedAsync();
    Task AddAsync(Atividade atividade);
    Task<Atividade?> GetByIdAsync(Guid id);
    Task UpdateAsync(Atividade atividade);
    Task DeleteAsync(Atividade atividade);
    Task RemoveRangeAsync(IEnumerable<Atividade> atividades);
}
