using System;

namespace Back.Application.DTOs.Aluno;

public record AlunoResponse(Guid Id, string Nome, string Email, string Matricula, string Role);
