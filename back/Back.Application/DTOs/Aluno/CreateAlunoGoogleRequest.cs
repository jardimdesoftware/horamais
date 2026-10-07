using System.ComponentModel.DataAnnotations;

namespace Back.Application.DTOs.Aluno;

public record CreateAlunoGoogleRequest(
    [Required] string RegistrationTicket,
    [Required] string Nome,
    [Required] string Matricula,
    [Required] string TurmaCodigo
);
