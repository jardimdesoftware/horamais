using System.ComponentModel.DataAnnotations;

namespace Back.Domain.Entities.Turma;

public class Turma
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O campo Período é obrigatório.")]
    public string? Periodo { get; set; }

    [Required(ErrorMessage = "O campo Turno é obrigatório.")]
    public string? Turno { get; set; }

    [Required(ErrorMessage = "O código da turma é obrigatório.")]
    [StringLength(6, MinimumLength = 6)]
    public string? Codigo { get; set; }

    public bool CodigoAtivo { get; set; } = true;

    public bool PossuiExtensao { get; set; }

    public int? MaximoHorasExtensao { get; set; }

    [Required(ErrorMessage = "O campo CursoId é obrigatório.")]
    public Guid CursoId { get; set; }

    public Curso.Curso? Curso { get; private set; }

    public ICollection<Aluno.Aluno> Alunos { get; private set; }

    internal Turma()
    {
        Alunos = new List<Aluno.Aluno>();
    }

    internal Turma(Guid id, Guid cursoId)
    {
        Id = id;
        CursoId = cursoId;
        Alunos = new List<Aluno.Aluno>();
    }
}