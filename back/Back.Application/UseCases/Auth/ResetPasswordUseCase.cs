using Back.Application.DTOs.Auth;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Auth
{
    public class ResetPasswordUseCase
    {
        private readonly IIdentityLookupService _identityLookup;
        private readonly IResetPasswordRepository _repo;
        private readonly UserManager<IdentityUser> _userManager;

        public ResetPasswordUseCase(
            IIdentityLookupService identityLookup,
            IResetPasswordRepository repo,
            UserManager<IdentityUser> userManager)
        {
            _identityLookup = identityLookup;
            _repo = repo;
            _userManager = userManager;
        }

        public async Task<ResetPasswordResponseDto> ExecuteAsync(ResetPasswordRequestDto dto)
        {
            var user = await _identityLookup.GetByEmailAsync(dto.Email);
            if (user == null)
                throw new InvalidOperationException("Código inválido ou expirado.");
            if (await _userManager.IsInRoleAsync(user, "ALUNO"))
                throw new InvalidOperationException("Código inválido ou expirado.");

            var record = await _repo.GetActiveByUserAsync(user.Id);
            if (record == null || record.Attempts >= 5)
                throw new InvalidOperationException("Código inválido ou expirado.");

            if (record.Code != dto.Code)
            {
                record.Attempts++;
                await _repo.UpdateAsync(record);
                await _repo.SaveChangesAsync();
                throw new InvalidOperationException("Código inválido ou expirado.");
            }

            var result = await _userManager.ResetPasswordAsync(user, record.IdentityResetToken, dto.NewPassword);
            if (!result.Succeeded)
            {
                var msg = string.Join("; ", System.Linq.Enumerable.Select(result.Errors, e => e.Description));
                throw new InvalidOperationException(msg);
            }

            record.Used = true;
            await _repo.UpdateAsync(record);
            await _repo.SaveChangesAsync();

            return new ResetPasswordResponseDto();
        }
    }
}
