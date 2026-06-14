using Back.Domain.Entities.Atividade;
using System;

namespace Back.Application.DTOs.Aluno;
public record AtividadeAlunoResumo(
    Guid AtividadeId,
    string Nome,
    string Grupo,
    string Categoria,
    string CategoriaKey,
    int CargaMaximaSemestral,
    int CargaMaximaCurso,
    int HorasConcluidas,
    string Tipo);