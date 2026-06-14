using System.ComponentModel.DataAnnotations;

namespace Back.Domain.Entities.Certificado;

public class Certificado
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O título da atividade é obrigatório.")]
    public string? TituloAtividade { get; set; }

    [Required(ErrorMessage = "A instituição é obrigatória.")]
    public string? Instituicao { get; set; }

    [Required(ErrorMessage = "O local é obrigatório.")]
    public string? Local { get; set; }

    [Required(ErrorMessage = "A categoria é obrigatória.")]
    public string? Categoria { get; set; }

    [Required(ErrorMessage = "O grupo é obrigatório.")]
    public string? Grupo { get; set; }

    [Required(ErrorMessage = "O período letivo é obrigatório.")]
    public string? PeriodoLetivo { get; set; }

    [Required(ErrorMessage = "A carga horária é obrigatória.")]
    public int CargaHoraria { get; set; }

    [Required(ErrorMessage = "A data de início é obrigatória.")]
    public DateTime DataInicio { get; set; }

    [Required(ErrorMessage = "A data de fim é obrigatória.")]
    public DateTime DataFim { get; set; }

    [Required(ErrorMessage = "O total de períodos é obrigatório.")]
    public int TotalPeriodos { get; set; }

    public string? Descricao { get; set; }

    [Required(ErrorMessage = "O anexo é obrigatório.")]
    public string? AnexoStorageKey { get; set; }

    public StatusCertificado Status { get; set; } = StatusCertificado.PENDENTE;

    public string? JustificativaRejeicao { get; set; }

    public int? CargaHorariaOriginal { get; set; }

    public bool CargaHorariaCorrigida { get; set; } = false;

    [Required(ErrorMessage = "O campo Tipo é obrigatório.")]
    public TipoCertificado Tipo { get; set; }

    [Required(ErrorMessage = "A relação com AlunoAtividade é obrigatória.")]
    public Guid AlunoAtividadeId { get; set; }
    public AlunoAtividade.AlunoAtividade? AlunoAtividade { get; set; }
}

public enum StatusCertificado
{
    PENDENTE,
    APROVADO,
    REPROVADO
}

public enum TipoCertificado
{
    EXTENSAO,
    COMPLEMENTAR
}
