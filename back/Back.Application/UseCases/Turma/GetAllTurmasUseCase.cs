using Back.Application.DTOs.Turma;
using Back.Application.Interfaces.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Turma;

public class GetAllTurmasUseCase
{
    private readonly ITurmaRepository _repo;

    public GetAllTurmasUseCase(ITurmaRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<TurmaResponse>> ExecuteAsync()
    {
        var turmas = await _repo.GetAllAsync();
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
