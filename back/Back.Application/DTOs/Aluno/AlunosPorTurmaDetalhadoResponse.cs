using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Back.Application.DTOs.Aluno;
public record AlunoPorTurmaDetalhadoResponse(
    Guid Id,
    string Nome,
    string Email,
    string Matricula,
    bool IsAtivo,
    int TotalHorasExtensao,
    int TotalHorasComplementar,
    int MaximoHorasExtensao,
    int MaximoHorasComplementar,
    double PorcentagemConclusao
);
