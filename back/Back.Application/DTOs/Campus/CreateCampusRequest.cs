using System.ComponentModel.DataAnnotations;

namespace Back.Application.DTOs.Campus;

public class CreateCampusRequest
{
    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O Nome deve ter entre 2 e 100 caracteres.")]
    public string? Nome { get; set; }

    [Required(ErrorMessage = "O campo Cidade é obrigatório.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "A Cidade deve ter entre 2 e 100 caracteres.")]
    public string? Cidade { get; set; }
}
