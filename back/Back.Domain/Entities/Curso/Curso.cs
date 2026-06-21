using System.ComponentModel.DataAnnotations;

namespace Back.Domain.Entities.Curso;

public class Curso
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O campo Nome do curso é obrigatório.")]
    public string? Nome { get; set; }

    /// <summary>
    /// Duração regular do curso em períodos letivos (semestres). Base para o
    /// cálculo do ritmo esperado de horas complementares (máximo ÷ duração).
    /// </summary>
    public int DuracaoEmPeriodos { get; set; } = 8;

    public Guid CampusId { get; set; }
    public Campus.Campus? Campus { get; set; }

    public ICollection<Turma.Turma> Turmas { get; set; } = new List<Turma.Turma>();

    public ICollection<Coordenador.Coordenador> Coordenadores { get; set; } = new List<Coordenador.Coordenador>();
}
