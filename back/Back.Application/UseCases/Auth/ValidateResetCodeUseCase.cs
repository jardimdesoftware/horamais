using Back.Application.DTOs.Auth;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Auth
{
    public class ValidateResetCodeUseCase
    {
        private readonly IIdentityLookupService _identityLookup;
        private readonly IResetPasswordRepository _repo;

        public ValidateResetCodeUseCase(IIdentityLookupService identityLookup, IResetPasswordRepository repo)
        {
            _identityLookup = identityLookup;
            _repo = repo;
        }

        public async Task<ValidateCodeResponseDto> ExecuteAsync(ValidateCodeRequestDto dto)
        {
            var user = await _identityLookup.GetByEmailAsync(dto.Email);
            if (user == null)
                return new ValidateCodeResponseDto { Valid = false, Message = "Código inválido ou expirado." };

            var record = await _repo.GetActiveByUserAsync(user.Id);
            if (record == null)
                return new ValidateCodeResponseDto { Valid = false, Message = "Código inválido ou expirado." };

            if (record.Attempts >= 5)
                return new ValidateCodeResponseDto { Valid = false, Message = "Código inválido ou expirado." };
            if (record.Code != dto.Code)
            {
                record.Attempts++;
                await _repo.UpdateAsync(record);
                await _repo.SaveChangesAsync();
                return new ValidateCodeResponseDto { Valid = false, Message = "Código inválido ou expirado." };
            }

            return new ValidateCodeResponseDto { Valid = true, Message = "Código válido." };
        }
    }
}
