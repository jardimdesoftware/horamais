using System.ComponentModel.DataAnnotations;

namespace Back.Application.DTOs.Coordenador;

public class UpdateCoordenadorSelfRequest
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string NumeroPortaria { get; set; } = string.Empty;

    [Required]
    public string DOU { get; set; } = string.Empty;

    /// <summary>
    /// Opcional: Preencha apenas se desejar redefinir a senha.
    /// </summary>
    public string? Senha { get; set; }
}