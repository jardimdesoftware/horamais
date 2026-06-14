using System;

namespace Back.Application.DTOs.Aluno
{
    public class UpdateAlunoResponse
    {
        public Guid Id { get; }
        public string Nome { get; }
        public string Email { get; }

        public UpdateAlunoResponse(Guid id, string nome, string email)
        {
            Id = id;
            Nome = nome;
            Email = email;
        }
    }
}