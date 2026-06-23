using Back.Application.DTOs.Auth;
using Back.Application.Interfaces;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Auth;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Auth
{
    /// <summary>
    /// Reenvia o código de verificação de e-mail para um cadastro ainda pendente.
    /// Resposta neutra: não revela se o e-mail existe ou já está confirmado.
    /// </summary>
    public class ReenviarVerificacaoUseCase
    {
        private const string Assunto = "Confirme seu e-mail — hora+";

        private readonly IIdentityLookupService _identityLookup;
        private readonly IEmailVerificationRepository _repo;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _templateService;

        public ReenviarVerificacaoUseCase(
            IIdentityLookupService identityLookup,
            IEmailVerificationRepository repo,
            IEmailService emailService,
            IEmailTemplateService templateService)
        {
            _identityLookup = identityLookup;
            _repo = repo;
            _emailService = emailService;
            _templateService = templateService;
        }

        public async Task<ResendVerificationResponseDto> ExecuteAsync(ResendVerificationRequestDto dto)
        {
            var user = await _identityLookup.GetByEmailAsync(dto.Email);

            // Resposta idempotente: nada a fazer se o usuário não existe ou já confirmou.
            if (user == null || user.EmailConfirmed)
                return new ResendVerificationResponseDto();

            // Invalida o código ativo anterior, se houver.
            var active = await _repo.GetActiveByUserAsync(user.Id);
            if (active != null)
            {
                active.Used = true;
                await _repo.UpdateAsync(active);
            }

            var codigo = GerarCodigoSeisDigitos();
            await _repo.AddAsync(new EmailVerificationCode
            {
                Id = Guid.NewGuid(),
                IdentityUserId = user.Id,
                Code = codigo,
                ExpiresAtUtc = DateTime.UtcNow.AddHours(24)
            });
            await _repo.SaveChangesAsync();

            var corpo = _templateService.RenderVerificacaoEmail(user.UserName ?? dto.Email, codigo);
            await _emailService.EnviarEmailAsync(dto.Email, Assunto, corpo);

            return new ResendVerificationResponseDto();
        }

        private static string GerarCodigoSeisDigitos()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            var value = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
            return value.ToString("D6");
        }
    }
}
