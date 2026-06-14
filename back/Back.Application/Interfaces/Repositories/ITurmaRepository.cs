using Back.Domain.Entities.Aluno;
using Back.Domain.Entities.Turma;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.Interfaces.Repositories;

public interface ITurmaRepository
{
    Task AddAsync(Turma turma);
    Task<IEnumerable<Turma>> GetAllAsync();
    Task<Turma?> GetByIdAsync(Guid id);
    Task<Turma?> GetByCodigoAsync(string codigo);
    Task<Turma?> GetByIdentifierAsync(string identifier);
    Task<Turma?> GetByIdentifierTrackedAsync(string identifier);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExistsByCodigoAsync(string codigo);
    Task<bool> ExistsByCursoPeriodoTurnoAsync(Guid cursoId, string periodo, string turno, Guid? ignorarId = null);
    Task<IEnumerable<Aluno>> GetAlunosByTurmaAsync(Guid turmaId);
    Task<IEnumerable<Turma>> GetByCursoIdAsync(Guid cursoId);
    Task<IEnumerable<Turma>> GetByCursoIdTrackedAsync(Guid cursoId);
    Task RemoveRangeAsync(IEnumerable<Turma> turmas);
    Task<Turma?> GetByIdTrackedAsync(Guid id);
    Task UpdateAsync(Turma turma);
    Task DeleteAsync(Turma turma);
    Task<IEnumerable<string>> GetDistinctPeriodosAsync();
}
