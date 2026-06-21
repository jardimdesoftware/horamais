using Back.Application.Common;
using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Certificado;

/// <summary>
/// Retorna os períodos letivos em que o aluno autenticado pode registrar horas:
/// de max(2019.2, período de ingresso) até o período corrente. Usado para
/// popular o seletor de período no formulário, impedindo seleção retroativa.
/// </summary>
public class GetPeriodosLetivosValidosDoAlunoUseCase
{
    private readonly IAlunoRepository _alunoRepo;

    public GetPeriodosLetivosValidosDoAlunoUseCase(IAlunoRepository alunoRepo)
    {
        _alunoRepo = alunoRepo;
    }

    public async Task<IReadOnlyList<string>> ExecuteAsync(string identityUserId)
    {
        var aluno = await _alunoRepo.GetByIdentityUserIdAsync(identityUserId)
            ?? throw new InvalidOperationException("Aluno não encontrado.");

        var periodoIngresso = await _alunoRepo.GetPeriodoIngressoAsync(aluno.Id);
        return PeriodoLetivo.ListarValidosParaAluno(periodoIngresso);
    }
}
