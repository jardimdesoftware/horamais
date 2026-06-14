using System.ComponentModel.DataAnnotations;

namespace Back.Domain.Entities.LembreteEmail;

public class LembreteEmail
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "CursoId é obrigatório.")]
    public Guid CursoId { get; set; }

    [Required(ErrorMessage = "A data do lembrete é obrigatória.")]
    public DateTime Data { get; set; }

    public string? MensagemPersonalizada { get; set; }

    public bool Enviado { get; set; } = false;

    public DateTime? EnviadoEm { get; set; }
}
