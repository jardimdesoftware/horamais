using Back.Application.DTOs.Auth;
using Back.Application.Interfaces;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Auth;
using Microsoft.AspNetCore.Identity;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Auth
{
    public class ForgotPasswordUseCase
    {
        private readonly IIdentityLookupService _identityLookup;
        private readonly IResetPasswordRepository _repo;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _templateService;

        public ForgotPasswordUseCase(
            IIdentityLookupService identityLookup,
            IResetPasswordRepository repo,
            UserManager<IdentityUser> userManager,
            IEmailService emailService,
            IEmailTemplateService templateService)
        {
            _identityLookup = identityLookup;
            _repo = repo;
            _userManager = userManager;
            _emailService = emailService;
            _templateService = templateService;
        }

        public async Task<ForgotPasswordResponseDto> ExecuteAsync(ForgotPasswordRequestDto dto)
        {
            var user = await _identityLookup.GetByEmailAsync(dto.Email);
            if (user == null)
            {
                // Resposta idempotente: não revela existência do e-mail
                return new ForgotPasswordResponseDto();
            }

            // Invalida código ativo anterior
            var active = await _repo.GetActiveByUserAsync(user.Id);
            if (active != null)
            {
                active.Used = true;
                await _repo.UpdateAsync(active);
                await _repo.SaveChangesAsync();
            }

            var sixDigit = GenerateSixDigitCode();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var entity = new ResetPasswordCode
            {
                Id = Guid.NewGuid(),
                IdentityUserId = user.Id,
                Code = sixDigit,
                IdentityResetToken = token,
                ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10)
            };

            await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();

            var subject = "Código de verificação — Redefinição de Senha";
            var body = _templateService.RenderResetSenha(user.UserName ?? dto.Email, sixDigit);

            await _emailService.EnviarEmailAsync(dto.Email, subject, body);

            return new ForgotPasswordResponseDto();
        }

        private static string GenerateSixDigitCode()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var value = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
            return value.ToString("D6");
        }
    }
}
