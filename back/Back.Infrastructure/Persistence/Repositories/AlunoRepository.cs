using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Aluno;
using Back.Domain.Entities.AlunoAtividade;
using Back.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Infrastructure.Persistence.Repositories;

public class AlunoRepository : IAlunoRepository
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    public AlunoRepository(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task AddAsync(Aluno aluno)
    {
        _context.Alunos.Add(aluno);
        await _context.SaveChangesAsync();
    }

    public async Task<Aluno?> GetByIdAsync(Guid id)
    {
        return await _context.Alunos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }
    public async Task<Aluno?> GetByIdentityUserIdAsync(string identityUserId)
    {
        return await _context.Alunos
            .FirstOrDefaultAsync(a => a.IdentityUserId == identityUserId);
    }
    public async Task<IEnumerable<Aluno>> GetAllWithAtividadesAsync()
    {
        return await _context.Alunos
            .Include(a => a.Turma)
            .Include(a => a.Atividades)
                .ThenInclude(aa => aa.Atividade)
            .ToListAsync();
    }

    public async Task<Aluno?> GetByIdWithAtividadesAsync(Guid id)
    {
        return await _context.Alunos
            .Include(a => a.Turma)
            .Include(a => a.Atividades)
                .ThenInclude(aa => aa.Atividade)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task UpdateAsync(Aluno aluno)
    {
        _context.Alunos.Update(aluno);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteAsync(Aluno aluno)
    {
        _context.Alunos.Remove(aluno);
        await _context.SaveChangesAsync();
    }
    public async Task DeleteComIdentityAsync(Guid alunoId)
    {
        var aluno = await _context.Alunos.FirstOrDefaultAsync(a => a.Id == alunoId);

        if (aluno == null) return;

        // Remove o usuário da AspNetUsers
        if (!string.IsNullOrWhiteSpace(aluno.IdentityUserId))
        {
            var identityUser = await _userManager.FindByIdAsync(aluno.IdentityUserId);
            if (identityUser != null)
            {
                await _userManager.DeleteAsync(identityUser);
            }
        }

        // Remove o aluno
        _context.Alunos.Remove(aluno);
        await _context.SaveChangesAsync();
    }
    public async Task<Aluno?> GetByIdentityUserIdWithAtividadesAsync(string identityUserId)
    {
        return await _context.Alunos
            .Include(a => a.Turma)
            .Include(a => a.Atividades)
                .ThenInclude(aa => aa.Atividade)
            .FirstOrDefaultAsync(a => a.IdentityUserId == identityUserId);
    }
    public async Task<IEnumerable<AlunoAtividade>> GetAtividadesByAlunoIdAsync(Guid alunoId)
    {
        return await _context.AlunoAtividades
            .Include(a => a.Atividade)
            .Where(a => a.AlunoId == alunoId)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<IEnumerable<Aluno>> GetAllComTurmaEAtividadesAsync()
    {
        return await _context.Alunos
            .Where(a => a.IsAtivo && a.Turma != null)
            .Include(a => a.Turma)
                .ThenInclude(t => t!.Curso)
            .Include(a => a.Atividades)
                .ThenInclude(aa => aa.Atividade)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<IEnumerable<Aluno>> GetAlunosPorCursoComDetalhesAsync(Guid cursoId)
    {
        return await _context.Alunos
            .Where(a => a.Turma != null && a.Turma.CursoId == cursoId)
            .Include(a => a.Turma)
                .ThenInclude(t => t!.Curso)
            .Include(a => a.Atividades)
                .ThenInclude(aa => aa.Atividade)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<IEnumerable<Aluno>> GetAllAsync()
    {
        return await _context.Alunos
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Aluno>> GetByCursoIdAsync(Guid cursoId)
    {
        return await _context.Alunos
            .Where(a => a.Turma!.CursoId == cursoId)
            .AsNoTracking()
            .ToListAsync();
    }
    public async Task<IEnumerable<Aluno>> GetByTurmaIdTrackedAsync(Guid turmaId)
    {
        return await _context.Alunos
            .Where(a => a.TurmaId == turmaId)
            .ToListAsync();
    }
    public async Task<IEnumerable<AlunoAtividade>> GetByAlunoIdTrackedAsync(Guid alunoId)
    {
        return await _context.AlunoAtividades
            .Where(aa => aa.AlunoId == alunoId)
            .ToListAsync();
    }

}
