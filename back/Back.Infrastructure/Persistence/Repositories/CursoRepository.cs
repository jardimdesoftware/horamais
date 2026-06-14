using Back.Application.DTOs.Curso;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Curso;
using Back.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Infrastructure.Persistence.Repositories;

public class CursoRepository : ICursoRepository
{
    private readonly ApplicationDbContext _context;

    public CursoRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Curso curso)
    {
        _context.Cursos.Add(curso);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Curso>> GetAllAsync(Guid? campusId = null)
    {
        var query = _context.Cursos.AsNoTracking();

        if (campusId is { } id)
            query = query.Where(c => c.CampusId == id);

        return await query.ToListAsync();
    }

    public async Task<Curso?> GetByIdAsync(Guid id)
    {
        return await _context.Cursos
            .Include(c => c.Campus)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<CursoResumoResponse>> GetResumoCursosAsync()
    {
        return await _context.Cursos
            .Include(c => c.Campus)
            .Include(c => c.Turmas!)
                .ThenInclude(t => t.Alunos)
            .Select(c => new CursoResumoResponse(
                c.Id,
                c.Nome!,
                c.Turmas!.Count,
                c.Turmas!.SelectMany(t => t.Alunos).Count(),
                c.CampusId,
                c.Campus!.Nome!
            ))
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task UpdateAsync(Curso curso)
    {
        _context.Cursos.Update(curso);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Curso curso)
    {
        _context.Cursos.Remove(curso);
        await _context.SaveChangesAsync();
    }

    public async Task<Curso?> GetByIdToUpdateAsync(Guid id)
    {
        return await _context.Cursos.FindAsync(id);
    }
}
