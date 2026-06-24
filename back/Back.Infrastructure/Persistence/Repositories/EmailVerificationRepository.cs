using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Auth;
using Back.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Back.Infrastructure.Persistence.Repositories
{
    public class EmailVerificationRepository : IEmailVerificationRepository
    {
        private readonly ApplicationDbContext _ctx;

        public EmailVerificationRepository(ApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<EmailVerificationCode?> GetActiveByUserAsync(string identityUserId)
        {
            var now = System.DateTime.UtcNow;
            return await _ctx.EmailVerificationCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdentityUserId == identityUserId && !x.Used && x.ExpiresAtUtc > now);
        }

        public async Task<EmailVerificationCode?> GetByUserAndCodeAsync(string identityUserId, string code)
        {
            return await _ctx.EmailVerificationCodes
                .FirstOrDefaultAsync(x => x.IdentityUserId == identityUserId && x.Code == code && !x.Used);
        }

        public async Task AddAsync(EmailVerificationCode entity)
        {
            await _ctx.EmailVerificationCodes.AddAsync(entity);
        }

        public Task UpdateAsync(EmailVerificationCode entity)
        {
            _ctx.EmailVerificationCodes.Update(entity);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync() => await _ctx.SaveChangesAsync();
    }
}
