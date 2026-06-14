using System.ComponentModel.DataAnnotations;

namespace Back.Domain.Entities.LimiteHorasAluno;

public class LimiteHorasAluno
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O campo MaximoHorasComplementar é obrigatório.")]
    public int MaximoHorasComplementar { get; set; }

    [Required(ErrorMessage = "O campo CursoId é obrigatório.")]
    public Guid CursoId { get; set; }
}
