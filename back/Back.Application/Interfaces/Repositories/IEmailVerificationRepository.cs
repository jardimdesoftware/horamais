using Back.Domain.Entities.Auth;
using System;
using System.Threading.Tasks;

namespace Back.Application.Interfaces.Repositories
{
    public interface IEmailVerificationRepository
    {
        Task<EmailVerificationCode?> GetActiveByUserAsync(string identityUserId);
        Task<EmailVerificationCode?> GetByUserAndCodeAsync(string identityUserId, string code);
        Task AddAsync(EmailVerificationCode entity);
        Task UpdateAsync(EmailVerificationCode entity);
        Task SaveChangesAsync();
    }
}
