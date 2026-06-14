using System;
using System.ComponentModel.DataAnnotations;

namespace Back.Application.DTOs.Curso;

public class CreateCursoComLimiteHorasRequest
{
    public string? NomeCurso { get; set; }
    public int MaximoHorasComplementar { get; set; }

    [Required(ErrorMessage = "O campo Campus é obrigatório.")]
    public Guid CampusId { get; set; }
}
