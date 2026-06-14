using System.ComponentModel.DataAnnotations;

namespace Back.Application.DTOs.Certificado;

public class ReprovarCertificadoRequest
{
    [Required(ErrorMessage = "A justificativa é obrigatória.")]
    [MinLength(10, ErrorMessage = "A justificativa deve ter no mínimo 10 caracteres.")]
    public string Justificativa { get; set; } = string.Empty;
}
