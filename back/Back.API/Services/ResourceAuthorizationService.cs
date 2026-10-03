using System.Security.Claims;
using Back.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace Back.API.Services;

/// <summary>Checks access to a record against the current Identity user, not a client-supplied ID.</summary>
public sealed class ResourceAuthorizationService(ApplicationDbContext context)
{
    private static string UserId(ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? throw new UnauthorizedAccessException("Usuário não autenticado.");

    public async Task<Guid> CoordinatorCourseIdAsync(ClaimsPrincipal user)
    {
        if (!user.IsInRole("COORDENADOR"))
            throw new UnauthorizedAccessException("Acesso não permitido.");

        return await context.Coordenadores
            .Where(c => c.IdentityUserId == UserId(user))
            .Select(c => (Guid?)c.CursoId)
            .SingleOrDefaultAsync()
            ?? throw new UnauthorizedAccessException("Coordenador não encontrado.");
    }

    public async Task EnsureCourseAsync(ClaimsPrincipal user, Guid cursoId)
    {
        if (await CoordinatorCourseIdAsync(user) != cursoId)
            throw new UnauthorizedAccessException("Este curso não pertence ao coordenador.");
    }

    public async Task EnsureTurmaAsync(ClaimsPrincipal user, string identifier)
    {
        var query = Guid.TryParse(identifier, out var id)
            ? context.Turmas.Where(t => t.Id == id)
            : context.Turmas.Where(t => t.Codigo == identifier);
        var cursoId = await query.Select(t => (Guid?)t.CursoId).FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Turma não encontrada.");
        await EnsureCourseAsync(user, cursoId);
    }

    public async Task EnsureAlunoAsync(ClaimsPrincipal user, Guid alunoId)
    {
        var cursoId = await context.Alunos
            .Where(a => a.Id == alunoId)
            .Select(a => (Guid?)a.Turma!.CursoId)
            .SingleOrDefaultAsync()
            ?? throw new KeyNotFoundException("Aluno não encontrado.");
        await EnsureCourseAsync(user, cursoId);
    }

    public async Task EnsureStudentOwnsAlunoAsync(ClaimsPrincipal user, Guid alunoId)
    {
        if (!user.IsInRole("ALUNO"))
            throw new UnauthorizedAccessException("Acesso não permitido.");

        var aluno = await context.Alunos
            .Where(a => a.Id == alunoId)
            .Select(a => new { a.IdentityUserId, a.IsAtivo })
            .SingleOrDefaultAsync()
            ?? throw new KeyNotFoundException("Aluno não encontrado.");
        if (aluno.IdentityUserId != UserId(user) || !aluno.IsAtivo)
            throw new UnauthorizedAccessException("Este aluno não pertence ao usuário autenticado.");
    }

    public async Task EnsureCoordinatorCanManageCertificadoAsync(ClaimsPrincipal user, Guid certificadoId)
    {
        var cursoId = await context.Certificados
            .Where(c => c.Id == certificadoId)
            .Select(c => (Guid?)c.AlunoAtividade!.Aluno!.Turma!.CursoId)
            .SingleOrDefaultAsync()
            ?? throw new KeyNotFoundException("Certificado não encontrado.");
        await EnsureCourseAsync(user, cursoId);
    }

    public async Task EnsureCanReadCertificadoAsync(ClaimsPrincipal user, Guid certificadoId)
    {
        var certificado = await context.Certificados
            .Where(c => c.Id == certificadoId)
            .Select(c => new
            {
                AlunoIdentityUserId = c.AlunoAtividade!.Aluno!.IdentityUserId,
                CursoId = c.AlunoAtividade.Aluno.Turma!.CursoId,
                AlunoAtivo = c.AlunoAtividade.Aluno.IsAtivo
            })
            .SingleOrDefaultAsync()
            ?? throw new KeyNotFoundException("Certificado não encontrado.");

        if (user.IsInRole("ADMIN")) return;
        if (user.IsInRole("ALUNO") && certificado.AlunoAtivo && certificado.AlunoIdentityUserId == UserId(user)) return;
        if (user.IsInRole("COORDENADOR") && certificado.CursoId == await CoordinatorCourseIdAsync(user)) return;
        throw new UnauthorizedAccessException("Acesso ao certificado não permitido.");
    }
}
