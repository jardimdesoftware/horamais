using Back.Domain.Entities.Auth;
using System;
using System.Threading.Tasks;

namespace Back.Application.Interfaces.Repositories
{
    public interface IResetPasswordRepository
    {
        Task<ResetPasswordCode?> GetActiveByUserAsync(string identityUserId);
        Task<ResetPasswordCode?> GetByUserAndCodeAsync(string identityUserId, string code);
        Task AddAsync(ResetPasswordCode entity);
        Task UpdateAsync(ResetPasswordCode entity);
        Task SaveChangesAsync();
    }
}
