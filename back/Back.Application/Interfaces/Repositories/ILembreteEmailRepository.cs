using Back.Domain.Entities.LembreteEmail;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.Interfaces.Repositories;

public interface ILembreteEmailRepository
{
    Task AddAsync(LembreteEmail lembrete);
    Task<LembreteEmail?> GetByIdAsync(Guid id);
    Task<IEnumerable<LembreteEmail>> GetByCursoIdAsync(Guid cursoId);
    Task<int> CountByCursoIdAsync(Guid cursoId);
    Task<IEnumerable<LembreteEmail>> GetPendentesAteAsync(DateTime data);
    Task UpdateAsync(LembreteEmail lembrete);
    Task DeleteAsync(LembreteEmail lembrete);
    Task SaveChangesAsync();
}
