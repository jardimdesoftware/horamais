using Back.Domain.Entities.Coordenador;
using System;
using System.Threading.Tasks;

namespace Back.Application.Interfaces.Repositories;

public interface ICoordenadorRepository
{
    Task AddAsync(Coordenador coordenador);
    Task<Coordenador?> GetByIdentityUserIdWithCursoAsync(string identityUserId);
    Task<Coordenador?> GetByCursoIdAsync(Guid cursoId);
    Task<Coordenador?> GetByIdAsync(Guid id);
    Task<Coordenador?> GetByIdentityUserIdAsync(string identityUserId);
    Task UpdateAsync(Coordenador coordenador);
    Task DeleteComIdentityAsync(Guid id);
}
