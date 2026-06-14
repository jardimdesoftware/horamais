using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.LimiteHorasAluno;
using Back.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Infrastructure.Persistence.Repositories;

public class LimiteHorasAlunoRepository : ILimiteHorasAlunoRepository
{
    private readonly ApplicationDbContext _context;

    public LimiteHorasAlunoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<LimiteHorasAluno?> GetByCursoIdAsync(Guid cursoId)
    {
        return await _context.LimitesHoras
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.CursoId == cursoId);
    }
    public async Task AddAsync(LimiteHorasAluno limite)
    {
        _context.LimitesHoras.Add(limite);
        await _context.SaveChangesAsync();
    }
    public async Task<IEnumerable<LimiteHorasAluno>> GetAllAsync()
    {
        return await _context.LimitesHoras.AsNoTracking().ToListAsync();
    }
    public async Task UpdateAsync(LimiteHorasAluno limite)
    {
        _context.LimitesHoras.Update(limite);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(LimiteHorasAluno limite)
    {
        _context.LimitesHoras.Remove(limite);
        await _context.SaveChangesAsync();
    }

    public async Task<LimiteHorasAluno?> GetByCursoIdToUpdateAsync(Guid cursoId)
    {
        return await _context.LimitesHoras
            .FirstOrDefaultAsync(x => x.CursoId == cursoId);
    }
}
