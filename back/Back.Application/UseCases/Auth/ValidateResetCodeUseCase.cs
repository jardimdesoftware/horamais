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
                return new ValidateCodeResponseDto { Valid = true, Message = "Se houver conta, o código será verificado." };

            var record = await _repo.GetByUserAndCodeAsync(user.Id, dto.Code);
            if (record == null)
                return new ValidateCodeResponseDto { Valid = false, Message = "Código inválido." };

            if (record.Used)
                return new ValidateCodeResponseDto { Valid = false, Message = "Código já utilizado." };

            if (record.ExpiresAtUtc <= DateTime.UtcNow)
                return new ValidateCodeResponseDto { Valid = false, Message = "Código expirado." };

            record.Attempts += 1;
            await _repo.UpdateAsync(record);
            await _repo.SaveChangesAsync();

            return new ValidateCodeResponseDto { Valid = true, Message = "Código válido." };
        }
    }
}
