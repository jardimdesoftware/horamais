using Back.Application.DTOs.Turma;
using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Turma;

public class ToggleCodigoUseCase
{
    private readonly ITurmaRepository _repo;

    public ToggleCodigoUseCase(ITurmaRepository repo)
    {
        _repo = repo;
    }

    public async Task<TurmaResponse> ExecuteAsync(string identifier)
    {
        var turma = await _repo.GetByIdentifierTrackedAsync(identifier);
        if (turma == null)
            throw new KeyNotFoundException("Turma não encontrada.");

        turma.CodigoAtivo = !turma.CodigoAtivo;
        await _repo.UpdateAsync(turma);

        var turmaCompleta = await _repo.GetByIdAsync(turma.Id);
        return new TurmaResponse(
            turmaCompleta!.Id,
            turmaCompleta.Periodo!,
            turmaCompleta.Turno!,
            turmaCompleta.Codigo!,
            turmaCompleta.CodigoAtivo,
            turmaCompleta.PossuiExtensao,
            turmaCompleta.MaximoHorasExtensao,
            turmaCompleta.CursoId,
            turmaCompleta.Curso?.Nome ?? "Curso não encontrado",
            turmaCompleta.Alunos.Count
        );
    }
}
