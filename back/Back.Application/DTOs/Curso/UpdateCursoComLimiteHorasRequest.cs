using System;
using System.ComponentModel.DataAnnotations;

namespace Back.Application.DTOs.Curso;

public class UpdateCursoComLimiteHorasRequest
{
    [Required(ErrorMessage = "O nome do curso é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome do curso deve ter entre 3 e 100 caracteres.")]
    public string NomeCurso { get; set; } = string.Empty;

    [Required(ErrorMessage = "O máximo de horas complementares é obrigatório.")]
    [Range(0, 9999, ErrorMessage = "O valor deve ser entre 0 e 9999.")]
    public int MaximoHorasComplementar { get; set; }

    [Required(ErrorMessage = "O campo Campus é obrigatório.")]
    public Guid CampusId { get; set; }
}