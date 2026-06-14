using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Campus;
using Back.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Infrastructure.Persistence.Repositories;

public class CampusRepository : ICampusRepository
{
    private readonly ApplicationDbContext _context;

    public CampusRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Campus campus)
    {
        _context.Campi.Add(campus);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Campus>> GetAllAsync()
    {
        return await _context.Campi.AsNoTracking().ToListAsync();
    }

    public async Task<Campus?> GetByIdAsync(Guid id)
    {
        return await _context.Campi.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
    }
}
