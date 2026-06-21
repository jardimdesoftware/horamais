using System;
using System.ComponentModel.DataAnnotations;

namespace Back.Application.DTOs.Curso;

public class CreateCursoComLimiteHorasRequest
{
    public string? NomeCurso { get; set; }
    public int MaximoHorasComplementar { get; set; }

    [Range(1, 20, ErrorMessage = "A duração deve ser entre 1 e 20 períodos.")]
    public int DuracaoEmPeriodos { get; set; } = 8;

    [Required(ErrorMessage = "O campo Campus é obrigatório.")]
    public Guid CampusId { get; set; }
}
