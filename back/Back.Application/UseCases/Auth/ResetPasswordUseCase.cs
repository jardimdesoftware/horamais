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
                return new ResetPasswordResponseDto(); // resposta neutra

            var record = await _repo.GetByUserAndCodeAsync(user.Id, dto.Code);
            if (record == null || record.Used || record.ExpiresAtUtc <= DateTime.UtcNow)
                throw new InvalidOperationException("Código inválido ou expirado.");

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
