using Back.Domain.Entities.Atividade;
using System.ComponentModel.DataAnnotations;

namespace Back.Application.DTOs.Atividade;

public class CreateAtividadeRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public string? Nome { get; set; }

    [Required]
    [StringLength(100)]
    public string? Grupo { get; set; }

    [Required]
    [StringLength(100)]
    public string? Categoria { get; set; }

    [Required]
    [StringLength(50)]
    public string? CategoriaKey { get; set; }

    [Range(0, 999)]
    public int CargaMaximaSemestral { get; set; }

    [Range(0, 9999)]
    public int CargaMaximaCurso { get; set; }

    public TipoAtividade Tipo { get; set; }

    public bool PossuiCurricularizacaoExtensao { get; set; }

    [Range(1, 9999)]
    public int? HorasCurricularizacaoExtensao { get; set; }
}
