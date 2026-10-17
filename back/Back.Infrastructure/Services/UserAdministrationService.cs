using Back.Domain.Entities.Admin;
using Back.Domain.Entities.Auth;
using Back.Domain.Entities.Coordenador;
using Back.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Infrastructure.Services;

public record ManagedUser(string Id, string Nome, string Email, string Cargo, bool Ativo,
    bool EmailVerificado, string CargoAposRebaixamento, Guid? CursoId, string? NumeroPortaria, string? Dou);

public class UserAdministrationService(ApplicationDbContext db, UserManager<IdentityUser> users,
    IConfiguration configuration)
{
    public static bool IsInstitutional(string email) =>
        email.EndsWith("@ifpe.edu.br", StringComparison.OrdinalIgnoreCase) ||
        email.EndsWith("@discente.ifpe.edu.br", StringComparison.OrdinalIgnoreCase);

    public static void EnsureSuccess(IdentityResult result)
    {
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    // Called only after validating Google's signature, audience and verified-email claim.
    public async Task BootstrapVerifiedEmailAsync(string email)
    {
        var initialEmail = configuration["INITIAL_ADMIN_EMAIL"]?.Trim();
        if (string.IsNullOrWhiteSpace(initialEmail) || !IsInstitutional(initialEmail) ||
            !string.Equals(initialEmail, email, StringComparison.OrdinalIgnoreCase)) return;

        await using var transaction = await AccountManagementLock.AcquireAsync(db);
        if (await db.AdminInitializations.AnyAsync()) return;

        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser { Email = email, UserName = email, EmailConfirmed = true };
            EnsureSuccess(await users.CreateAsync(user));
            EnsureSuccess(await users.AddToRoleAsync(user, "USUARIO"));
        }
        else
        {
            if (!await IsActiveAsync(user.Id))
                throw new UnauthorizedAccessException("Conta inativa. Procure o administrador.");
            user.EmailConfirmed = true;
            EnsureSuccess(await users.UpdateAsync(user));
        }

        await PromoteAsync(user);
        db.AdminInitializations.Add(new AdminInitialization { Id = 1, CompletedAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<bool> IsActiveAsync(string id) =>
        !await db.UserAccesses.AnyAsync(x => x.IdentityUserId == id && !x.IsActive);

    public async Task<object> ListAsync(string? search, int page)
    {
        const int pageSize = 25;
        page = Math.Max(1, page);
        var query = db.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToUpperInvariant();
            query = query.Where(u => u.NormalizedEmail!.Contains(term) ||
                db.Alunos.Any(a => a.IdentityUserId == u.Id && a.Nome!.ToUpper().Contains(term)) ||
                db.Coordenadores.Any(c => c.IdentityUserId == u.Id && c.Nome.ToUpper().Contains(term)));
        }
        var total = await query.CountAsync();
        var pageUsers = await query.OrderBy(u => u.Email).ThenBy(u => u.Id)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        var ids = pageUsers.Select(u => u.Id).ToArray();
        var roles = await db.UserRoles.Where(x => ids.Contains(x.UserId))
            .Join(db.Roles, x => x.RoleId, r => r.Id, (x, r) => new { x.UserId, r.Name }).ToListAsync();
        var alunos = await db.Alunos.Where(x => ids.Contains(x.IdentityUserId!)).ToListAsync();
        var coords = await db.Coordenadores.Where(x => ids.Contains(x.IdentityUserId!)).ToListAsync();
        var disabled = await db.UserAccesses.Where(x => ids.Contains(x.IdentityUserId) && !x.IsActive)
            .Select(x => x.IdentityUserId).ToListAsync();
        var items = pageUsers.Select(u =>
        {
            var aluno = alunos.FirstOrDefault(a => a.IdentityUserId == u.Id);
            var coord = coords.FirstOrDefault(c => c.IdentityUserId == u.Id);
            var isAdmin = roles.Any(r => r.UserId == u.Id && r.Name == "ADMIN");
            var fallback = coord != null ? "COORDENADOR" : aluno != null ? "ALUNO" : "USUARIO";
            return new ManagedUser(u.Id, coord?.Nome ?? aluno?.Nome ?? u.Email!, u.Email!,
                isAdmin ? "ADMIN" : fallback, !disabled.Contains(u.Id) && (isAdmin || aluno?.IsAtivo != false),
                u.EmailConfirmed, fallback, coord?.CursoId, coord?.NumeroPortaria, coord?.DOU);
        }).ToArray();
        return new { items, total, page, pageSize };
    }

    public async Task AssignCoordinatorAsync(string actorId, string targetId, Guid courseId,
        string name, string ordinance, string dou)
    {
        ValidateCoordinatorDetails(courseId, name, ordinance, dou);
        await using var transaction = await AccountManagementLock.AcquireAsync(db);
        await EnsureActiveAdminAsync(actorId);
        var user = await users.FindByIdAsync(targetId)
            ?? throw new System.Collections.Generic.KeyNotFoundException("Usuário não encontrado.");
        await AssignCoordinatorLockedAsync(user, courseId, name, ordinance, dou, allowUnconfirmed: false);
        await transaction.CommitAsync();
    }

    public async Task CreateCoordinatorAsync(string actorId, string email, Guid courseId,
        string name, string ordinance, string dou)
    {
        ValidateCoordinatorDetails(courseId, name, ordinance, dou);
        email = email?.Trim() ?? "";
        if (!IsInstitutional(email))
            throw new ArgumentException("Informe um e-mail institucional do IFPE.");
        await using var transaction = await AccountManagementLock.AcquireAsync(db);
        await EnsureActiveAdminAsync(actorId);
        var user = await users.FindByEmailAsync(email);
        if (user is null)
        {
            user = new IdentityUser { Email = email, UserName = email, EmailConfirmed = false };
            EnsureSuccess(await users.CreateAsync(user));
            EnsureSuccess(await users.AddToRoleAsync(user, "USUARIO"));
        }
        await AssignCoordinatorLockedAsync(user, courseId, name, ordinance, dou, allowUnconfirmed: true);
        await transaction.CommitAsync();
    }

    private static void ValidateCoordinatorDetails(Guid courseId, string name, string ordinance, string dou)
    {
        if (courseId == Guid.Empty || string.IsNullOrWhiteSpace(name) ||
            string.IsNullOrWhiteSpace(ordinance) || string.IsNullOrWhiteSpace(dou))
            throw new ArgumentException("Informe curso, nome, portaria e DOU.");
    }

    private async Task EnsureActiveAdminAsync(string actorId)
    {
        var actor = await users.FindByIdAsync(actorId);
        if (actor is null || !actor.EmailConfirmed || !await IsActiveAsync(actorId) ||
            !await users.IsInRoleAsync(actor, "ADMIN"))
            throw new UnauthorizedAccessException("Permissão administrativa revogada.");
    }

    private async Task AssignCoordinatorLockedAsync(IdentityUser user, Guid courseId,
        string name, string ordinance, string dou, bool allowUnconfirmed)
    {
        var targetId = user.Id;
        if ((!allowUnconfirmed && !user.EmailConfirmed) || !IsInstitutional(user.Email ?? "") || !await IsActiveAsync(targetId) ||
            await users.IsInRoleAsync(user, "ADMIN") ||
            await db.Alunos.AnyAsync(a => a.IdentityUserId == targetId && !a.IsAtivo))
            throw new InvalidOperationException("A atribuição exige uma conta institucional ativa e sem cargo ADMIN.");
        if (!await db.Cursos.AnyAsync(c => c.Id == courseId))
            throw new InvalidOperationException("Curso não encontrado.");

        var coordinator = await db.Coordenadores.FirstOrDefaultAsync(c => c.IdentityUserId == targetId);
        if (coordinator is null)
        {
            coordinator = new CoordenadorBuilder().WithId(Guid.NewGuid()).WithIdentityUserId(targetId)
                .WithEmail(user.Email!).WithNome(name.Trim()).WithNumeroPortaria(ordinance.Trim())
                .WithDOU(dou.Trim()).WithCursoId(courseId).Build();
            db.Coordenadores.Add(coordinator);
        }
        else
        {
            coordinator.Nome = name.Trim();
            coordinator.Email = user.Email!;
            coordinator.NumeroPortaria = ordinance.Trim();
            coordinator.DOU = dou.Trim();
            coordinator.CursoId = courseId;
        }
        if (!await users.IsInRoleAsync(user, "COORDENADOR"))
            EnsureSuccess(await users.AddToRoleAsync(user, "COORDENADOR"));
        EnsureSuccess(await users.UpdateSecurityStampAsync(user));
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(string actorId, string targetId, bool? active, bool? administrator)
    {
        if (active is null && administrator is null) throw new ArgumentException("Informe uma alteração.");
        await using var transaction = await AccountManagementLock.AcquireAsync(db);
        // Re-check after taking the lock: a concurrent request may have revoked the actor.
        var actor = await users.FindByIdAsync(actorId);
        if (actor is null || !actor.EmailConfirmed || !await IsActiveAsync(actorId) ||
            !await users.IsInRoleAsync(actor, "ADMIN"))
            throw new UnauthorizedAccessException("Permissão administrativa revogada.");
        var user = await users.FindByIdAsync(targetId) ?? throw new System.Collections.Generic.KeyNotFoundException("Usuário não encontrado.");
        var isAdmin = await users.IsInRoleAsync(user, "ADMIN");
        var willBeAdmin = administrator ?? isAdmin;
        var willBeActive = active ?? await IsActiveAsync(targetId);
        if (isAdmin && (!willBeAdmin || !willBeActive))
        {
            var otherAdminExists = await db.UserRoles.Join(db.Roles, ur => ur.RoleId, r => r.Id,
                    (ur, r) => new { ur.UserId, r.Name })
                .AnyAsync(x => x.Name == "ADMIN" && x.UserId != targetId &&
                    db.Users.Any(u => u.Id == x.UserId && u.EmailConfirmed) &&
                    !db.UserAccesses.Any(a => a.IdentityUserId == x.UserId && !a.IsActive));
            if (!otherAdminExists)
                throw new InvalidOperationException("O sistema deve manter ao menos um ADMIN ativo. Promova outro usuário antes de continuar.");
        }

        if (administrator == true && !isAdmin)
        {
            if (!willBeActive || !user.EmailConfirmed || !IsInstitutional(user.Email ?? "") ||
                await db.Alunos.AnyAsync(a => a.IdentityUserId == targetId && !a.IsAtivo) && active != true)
                throw new InvalidOperationException("A promoção exige uma conta ativa com e-mail institucional verificado.");
            await PromoteAsync(user);
        }
        else if (administrator == false && isAdmin)
        {
            EnsureSuccess(await users.RemoveFromRoleAsync(user, "ADMIN"));
            var fallback = await db.Coordenadores.AnyAsync(c => c.IdentityUserId == targetId) ? "COORDENADOR" :
                await db.Alunos.AnyAsync(a => a.IdentityUserId == targetId) ? "ALUNO" : "USUARIO";
            if (!await users.IsInRoleAsync(user, fallback)) EnsureSuccess(await users.AddToRoleAsync(user, fallback));
            // Keep the account and its academic records; only remove administrative authority.
        }

        if (active.HasValue)
        {
            var access = await db.UserAccesses.FindAsync(targetId);
            if (access is null) db.UserAccesses.Add(new UserAccess { IdentityUserId = targetId, IsActive = active.Value });
            else access.IsActive = active.Value;
            var aluno = await db.Alunos.FirstOrDefaultAsync(a => a.IdentityUserId == targetId);
            if (aluno != null) aluno.IsAtivo = active.Value;
        }
        EnsureSuccess(await users.UpdateSecurityStampAsync(user));
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    private async Task PromoteAsync(IdentityUser user)
    {
        if (!await users.IsInRoleAsync(user, "ADMIN")) EnsureSuccess(await users.AddToRoleAsync(user, "ADMIN"));
        if (!await db.Admins.AnyAsync(a => a.IdentityUserId == user.Id))
            db.Admins.Add(new Admin { Id = Guid.NewGuid(), IdentityUserId = user.Id, Email = user.Email! });
        EnsureSuccess(await users.UpdateSecurityStampAsync(user));
    }
}
