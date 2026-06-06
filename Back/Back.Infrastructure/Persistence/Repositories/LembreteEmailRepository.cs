using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.LembreteEmail;
using Back.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Infrastructure.Persistence.Repositories;

public class LembreteEmailRepository : ILembreteEmailRepository
{
    private readonly ApplicationDbContext _context;

    public LembreteEmailRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(LembreteEmail lembrete)
    {
        _context.LembretesEmail.Add(lembrete);
        return Task.CompletedTask;
    }

    public async Task<LembreteEmail?> GetByIdAsync(Guid id)
    {
        return await _context.LembretesEmail.FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<LembreteEmail>> GetByCursoIdAsync(Guid cursoId)
    {
        return await _context.LembretesEmail
            .AsNoTracking()
            .Where(l => l.CursoId == cursoId)
            .OrderBy(l => l.Data)
            .ToListAsync();
    }

    public Task<int> CountByCursoIdAsync(Guid cursoId)
    {
        return _context.LembretesEmail.CountAsync(l => l.CursoId == cursoId);
    }

    public async Task<IEnumerable<LembreteEmail>> GetPendentesAteAsync(DateTime data)
    {
        return await _context.LembretesEmail
            .Where(l => !l.Enviado && l.Data <= data)
            .ToListAsync();
    }

    public Task UpdateAsync(LembreteEmail lembrete)
    {
        _context.LembretesEmail.Update(lembrete);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(LembreteEmail lembrete)
    {
        _context.LembretesEmail.Remove(lembrete);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
