using Back.Application.DTOs.Turma;
using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Turma;

public class GetTurmasByCursoIdUseCase
{
    private readonly ITurmaRepository _repo;

    public GetTurmasByCursoIdUseCase(ITurmaRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<TurmaResponse>> ExecuteAsync(Guid cursoId)
    {
        var turmas = await _repo.GetByCursoIdAsync(cursoId);

        return turmas.Select(t => new TurmaResponse(
            t.Id,
            t.Periodo!,
            t.Turno!,
            t.Codigo!,
            t.CodigoAtivo,
            t.PossuiExtensao,
            t.MaximoHorasExtensao,
            t.CursoId,
            t.Curso?.Nome ?? "Curso não encontrado",
            t.Alunos.Count
        ));
    }

}
