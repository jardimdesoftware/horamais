using Back.Domain.Entities.Aluno;
using Back.Domain.Entities.AlunoAtividade;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.Interfaces.Repositories;

public interface IAlunoRepository
{
    Task AddAsync(Aluno aluno);
    Task<Aluno?> GetByIdAsync(Guid id);
    Task<Aluno?> GetByIdentityUserIdAsync(string identityUserId);

    Task<IEnumerable<Aluno>> GetAllWithAtividadesAsync();
    Task<Aluno?> GetByIdWithAtividadesAsync(Guid id);
    Task UpdateAsync(Aluno aluno);
    Task DeleteAsync(Domain.Entities.Aluno.Aluno aluno);
    Task DeleteComIdentityAsync(Guid alunoId);
    Task<Aluno?> GetByIdentityUserIdWithAtividadesAsync(string identityUserId);
    Task<IEnumerable<AlunoAtividade>> GetAtividadesByAlunoIdAsync(Guid alunoId);
    Task<IEnumerable<Aluno>> GetAllComTurmaEAtividadesAsync();
    Task<IEnumerable<Aluno>> GetAlunosPorCursoComDetalhesAsync(Guid cursoId);
    Task<IEnumerable<Aluno>> GetAllAsync();
    Task<IEnumerable<Aluno>> GetByCursoIdAsync(Guid cursoId);
    Task<IEnumerable<Aluno>> GetByTurmaIdTrackedAsync(Guid turmaId);
}
