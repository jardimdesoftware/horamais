using Back.Application.DTOs.Aluno;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Atividade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Aluno;

public class GetAlunosEmRiscoUseCase
{
    private readonly IAlunoRepository _alunoRepo;
    private readonly ILimiteHorasAlunoRepository _limiteRepo;

    public GetAlunosEmRiscoUseCase(
        IAlunoRepository alunoRepo,
        ILimiteHorasAlunoRepository limiteRepo)
    {
        _alunoRepo = alunoRepo;
        _limiteRepo = limiteRepo;
    }

    public async Task<IEnumerable<AlunoEmRiscoResponse>> ExecuteAsync(double percentualMaximo = 100)
    {
        var alunos = await _alunoRepo.GetAllComTurmaEAtividadesAsync();
        var limites = (await _limiteRepo.GetAllAsync())
            .ToDictionary(l => l.CursoId);

        return alunos
            .Where(a => limites.ContainsKey(a.Turma!.CursoId))
            .Where(a => limites[a.Turma!.CursoId].MaximoHorasComplementar > 0)
            .Select(a =>
            {
                var limite = limites[a.Turma!.CursoId];
                var totalComplementar = a.Atividades
                    .Where(x => x.Atividade?.Tipo == TipoAtividade.COMPLEMENTAR)
                    .Sum(x => x.HorasConcluidas);
                var maximo = limite.MaximoHorasComplementar;
                var porcentagem = Math.Round((double)totalComplementar / maximo * 100, 2);
                var periodos = CalcularPeriodosDecorridos(a.Turma.Periodo);
                return (Aluno: a, TotalComplementar: totalComplementar, Maximo: maximo,
                        Porcentagem: porcentagem, Periodos: periodos);
            })
            .Where(x => x.Porcentagem <= percentualMaximo && x.Periodos.HasValue)
            .Select(x => new AlunoEmRiscoResponse(
                x.Aluno.Id,
                x.Aluno.Nome!,
                x.Aluno.Matricula!,
                x.Aluno.Turma!.Periodo!,
                x.Aluno.Turma.Codigo!,
                x.Aluno.Turma.Curso?.Nome ?? "",
                x.TotalComplementar,
                x.Maximo,
                x.Porcentagem,
                x.Periodos!.Value,
                Math.Round((double)x.TotalComplementar / x.Periodos!.Value, 2)
            ))
            .OrderBy(a => a.HorasPorPeriodo);
    }

    /// <summary>
    /// Calcula quantos períodos letivos se passaram desde o início da turma.
    /// Período no formato "YYYY.S" onde S é 1 (jan–jun) ou 2 (jul–dez).
    /// Retorna null se o período for nulo, mal formatado ou com semestre fora de [1,2].
    /// </summary>
    private static int? CalcularPeriodosDecorridos(string? periodo)
    {
        if (string.IsNullOrWhiteSpace(periodo)) return null;

        var partes = periodo.Split('.');
        if (partes.Length != 2
            || !int.TryParse(partes[0], out var anoInicio)
            || !int.TryParse(partes[1], out var semInicio)
            || semInicio < 1 || semInicio > 2)
            return null;

        var hoje = DateTime.UtcNow;
        var anoAtual = hoje.Year;
        var semAtual = hoje.Month <= 6 ? 1 : 2;

        var periodos = (anoAtual - anoInicio) * 2 + (semAtual - semInicio) + 1;
        return Math.Max(1, periodos);
    }
}
