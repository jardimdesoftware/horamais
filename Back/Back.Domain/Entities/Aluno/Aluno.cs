using System.ComponentModel.DataAnnotations;

namespace Back.Domain.Entities.Aluno;

public class Aluno
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    public string? Nome { get; set; }

    [Required(ErrorMessage = "O campo Email é obrigatório.")]
    [EmailAddress(ErrorMessage = "Email inválido.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "O campo Matrícula é obrigatório.")]
    public string? Matricula { get; set; }

    public bool JaBaixadoHorasComplementares { get; set; }
    public bool? JaBaixadoHorasExtensao { get; set; }

    // Garante que a secretaria seja notificada uma única vez por aluno e por tipo
    // de carga (extensão e complementar são tratados separadamente — PPC novo).
    public bool NotificadoSecretariaComplementar { get; set; }
    public bool NotificadoSecretariaExtensao { get; set; }

    [Required(ErrorMessage = "O campo TurmaId é obrigatório.")]
    public Guid TurmaId { get; set; }

    [Required]
    public string? IdentityUserId { get; set; }

    public bool IsAtivo { get; set; } = true;

    public Turma.Turma? Turma { get; private set; }

    public ICollection<AlunoAtividade.AlunoAtividade> Atividades { get; set; } = new List<AlunoAtividade.AlunoAtividade>();

    internal Aluno() { }

    internal Aluno(Guid id, string nome, string email, string matricula, Guid turmaId, string identityUserId)
    {
        Id = id;
        Nome = nome;
        Email = email;
        Matricula = matricula;
        TurmaId = turmaId;
        IdentityUserId = identityUserId;
        IsAtivo = true;
    }
}
