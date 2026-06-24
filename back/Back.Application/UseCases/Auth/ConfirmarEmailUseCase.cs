using Back.Application.DTOs.Auth;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Auth
{
    /// <summary>
    /// Confirma o e-mail de um cadastro pendente a partir do código de 6 dígitos
    /// enviado ao aluno. Ao confirmar, marca o usuário do Identity como
    /// <c>EmailConfirmed</c>, liberando o login.
    /// </summary>
    public class ConfirmarEmailUseCase
    {
        private readonly IIdentityLookupService _identityLookup;
        private readonly IEmailVerificationRepository _repo;
        private readonly UserManager<IdentityUser> _userManager;

        public ConfirmarEmailUseCase(
            IIdentityLookupService identityLookup,
            IEmailVerificationRepository repo,
            UserManager<IdentityUser> userManager)
        {
            _identityLookup = identityLookup;
            _repo = repo;
            _userManager = userManager;
        }

        public async Task<ConfirmEmailResponseDto> ExecuteAsync(ConfirmEmailRequestDto dto)
        {
            var user = await _identityLookup.GetByEmailAsync(dto.Email);
            if (user == null)
                throw new InvalidOperationException("Código inválido ou expirado.");

            if (user.EmailConfirmed)
                return new ConfirmEmailResponseDto { Message = "E-mail já confirmado." };

            var record = await _repo.GetByUserAndCodeAsync(user.Id, dto.Code);
            if (record == null || record.Used || record.ExpiresAtUtc <= DateTime.UtcNow)
                throw new InvalidOperationException("Código inválido ou expirado.");

            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var msg = string.Join("; ", System.Linq.Enumerable.Select(result.Errors, e => e.Description));
                throw new InvalidOperationException(msg);
            }

            record.Used = true;
            await _repo.UpdateAsync(record);
            await _repo.SaveChangesAsync();

            return new ConfirmEmailResponseDto();
        }
    }
}
