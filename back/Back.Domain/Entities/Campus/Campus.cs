using System.ComponentModel.DataAnnotations;

namespace Back.Domain.Entities.Campus;

public class Campus
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O campo Nome do campus é obrigatório.")]
    public string? Nome { get; set; }

    [Required(ErrorMessage = "O campo Cidade do campus é obrigatório.")]
    public string? Cidade { get; set; }

    public ICollection<Curso.Curso> Cursos { get; set; } = new List<Curso.Curso>();
}
