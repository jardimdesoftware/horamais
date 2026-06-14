using System;
using System.ComponentModel.DataAnnotations;

namespace Back.Application.DTOs.Aluno
{
    public class UpdateAlunoRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "Formato de e-mail inválido.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "A matrícula é obrigatória.")]
        public string Matricula { get; set; } = string.Empty;

        [Required(ErrorMessage = "A turma é obrigatória.")]
        public Guid TurmaId { get; set; }

        /// <summary>
        /// Opcional: Preencha apenas se desejar redefinir a senha do aluno.
        /// Se for null ou vazio, a senha atual será mantida.
        /// </summary>
        public string? Senha { get; set; }
    }
}