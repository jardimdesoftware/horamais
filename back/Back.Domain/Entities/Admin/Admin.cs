using System.ComponentModel.DataAnnotations;

namespace Back.Domain.Entities.Admin;

public class Admin
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O campo Email é obrigatório.")]
    [EmailAddress(ErrorMessage = "O campo Email deve ser válido.")]
    public string Email { get; set; }

    [Required]
    public string IdentityUserId { get; set; } // vínculo com Identity
}
