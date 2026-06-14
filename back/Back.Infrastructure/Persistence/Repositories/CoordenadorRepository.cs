using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Coordenador;
using Back.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Back.Infrastructure.Persistence.Repositories;

public class CoordenadorRepository : ICoordenadorRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    public CoordenadorRepository(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task AddAsync(Coordenador coordenador)
    {
        _context.Coordenadores.Add(coordenador);
        await _context.SaveChangesAsync();
    }
    public async Task<Coordenador?> GetByIdentityUserIdWithCursoAsync(string identityUserId)
    {
        return await _context.Coordenadores
            .Include(c => c.Curso)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdentityUserId == identityUserId);
    }
    public async Task<Coordenador?> GetByCursoIdAsync(Guid cursoId)
    {
        return await _context.Coordenadores
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CursoId == cursoId);
    }
    public async Task UpdateAsync(Coordenador coordenador)
    {
        _context.Coordenadores.Update(coordenador);
        await _context.SaveChangesAsync();
    }
    public async Task<Coordenador?> GetByIdAsync(Guid id)
    {
        // FindAsync é bom para buscar por chave primária
        return await _context.Coordenadores.FindAsync(id);
    }

    public async Task<Coordenador?> GetByIdentityUserIdAsync(string identityUserId)
    {
        // Este método RASTREIA a entidade (sem AsNoTracking) para permitir o update
        return await _context.Coordenadores
            .FirstOrDefaultAsync(c => c.IdentityUserId == identityUserId);
    }

    public async Task DeleteComIdentityAsync(Guid id)
    {
        var coordenador = await _context.Coordenadores.FirstOrDefaultAsync(a => a.Id == id);

        if (coordenador == null) return;

        // Remove o usuário da AspNetUsers
        if (!string.IsNullOrWhiteSpace(coordenador.IdentityUserId))
        {
            var identityUser = await _userManager.FindByIdAsync(coordenador.IdentityUserId);
            if (identityUser != null)
            {
                await _userManager.DeleteAsync(identityUser);
            }
        }

        // Remove o coordenador
        _context.Coordenadores.Remove(coordenador);
        await _context.SaveChangesAsync();
    }
}
