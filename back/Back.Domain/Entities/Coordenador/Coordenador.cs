using System.ComponentModel.DataAnnotations;

namespace Back.Domain.Entities.Coordenador;

public class Coordenador
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "O campo Nome é obrigatório.")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "O campo Número da Portaria é obrigatório.")]
    public string NumeroPortaria { get; set; }

    [Required(ErrorMessage = "O campo DOU é obrigatório.")]
    public string DOU { get; set; }

    [Required(ErrorMessage = "O campo Email é obrigatório.")]
    [EmailAddress(ErrorMessage = "O campo Email deve ser um endereço de email válido.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "O campo CursoId é obrigatório.")]
    public Guid CursoId { get; set; }
    [Required]
    public string IdentityUserId { get; set; }
    public Curso.Curso Curso { get; private set; }

    internal Coordenador() { }

    internal Coordenador(Guid id, string nome, string email, string identityUserId, Guid cursoId)
    {
        Id = id;
        Nome = nome;
        Email = email;
        IdentityUserId = identityUserId;
        CursoId = cursoId;
    }
}
