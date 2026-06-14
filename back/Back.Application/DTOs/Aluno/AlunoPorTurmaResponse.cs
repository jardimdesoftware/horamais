using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Back.Application.DTOs.Aluno;
public record AlunoPorTurmaResponse(
    Guid Id,
    string Nome,
    string Email,
    string Matricula,
    bool IsAtivo
);