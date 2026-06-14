using System;
using System.ComponentModel.DataAnnotations;

namespace Back.Application.DTOs.Turma;

public class UpdateTurmaRequest
{
    [Required(ErrorMessage = "O período é obrigatório.")]
    [StringLength(20)]
    public string Periodo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O turno é obrigatório.")]
    [StringLength(20)]
    public string Turno { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informar se possui extensão é obrigatório.")]
    public bool PossuiExtensao { get; set; }

    public int? MaximoHorasExtensao { get; set; }

    [Required(ErrorMessage = "O ID do curso é obrigatório.")]
    public Guid CursoId { get; set; }
}