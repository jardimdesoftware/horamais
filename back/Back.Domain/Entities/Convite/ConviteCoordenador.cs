using System.ComponentModel.DataAnnotations;

namespace Back.Domain.Entities.Convite;

public class ConviteCoordenador
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "CursoId é obrigatório.")]
    public Guid CursoId { get; set; }

    [Required]
    public string Token { get; set; } = string.Empty;

    public DateTime ExpiraEm { get; set; }
    public bool Usado { get; set; } = false;
}
