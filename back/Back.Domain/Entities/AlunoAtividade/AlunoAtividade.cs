using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Back.Domain.Entities.Aluno;

namespace Back.Domain.Entities.AlunoAtividade;

public class AlunoAtividade
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid AlunoId { get; set; }

    [Required]
    public Guid AtividadeId { get; set; }

    [Required]
    public int HorasConcluidas { get; set; } = 0;

    [ForeignKey(nameof(AlunoId))]
    public Aluno.Aluno? Aluno { get; set; }

    [ForeignKey(nameof(AtividadeId))]
    public Atividade.Atividade? Atividade { get; set; }
}
