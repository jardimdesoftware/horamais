using System;
using System.ComponentModel.DataAnnotations;

namespace Back.Domain.Entities.Auth
{
    public class ResetPasswordCode
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string IdentityUserId { get; set; } = default!;

        [Required]
        [MaxLength(6)]
        public string Code { get; set; } = default!; // 6 dígitos

        [Required]
        public string IdentityResetToken { get; set; } = default!; // token do Identity para reset

        [Required]
        public DateTime ExpiresAtUtc { get; set; } // expiração do código

        public bool Used { get; set; } = false; // marcado após redefinir

        public int Attempts { get; set; } = 0; // tentativas de validação

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
