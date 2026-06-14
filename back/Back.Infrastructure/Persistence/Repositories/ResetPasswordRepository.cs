using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Auth;
using Back.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Back.Infrastructure.Persistence.Repositories
{
    public class ResetPasswordRepository : IResetPasswordRepository
    {
        private readonly ApplicationDbContext _ctx;

        public ResetPasswordRepository(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<ResetPasswordCode?> GetActiveByUserAsync(string identityUserId)
        {
            var now = System.DateTime.UtcNow;
            return await _ctx.ResetPasswordCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdentityUserId == identityUserId && !x.Used && x.ExpiresAtUtc > now);
        }

        public async Task<ResetPasswordCode?> GetByUserAndCodeAsync(string identityUserId, string code)
        {
            return await _ctx.ResetPasswordCodes
                .FirstOrDefaultAsync(x => x.IdentityUserId == identityUserId && x.Code == code && !x.Used);
        }

        public async Task AddAsync(ResetPasswordCode entity)
        {
            await _ctx.ResetPasswordCodes.AddAsync(entity);
        }

        public Task UpdateAsync(ResetPasswordCode entity)
        {
            _ctx.ResetPasswordCodes.Update(entity);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync() => await _ctx.SaveChangesAsync();
    }
}
