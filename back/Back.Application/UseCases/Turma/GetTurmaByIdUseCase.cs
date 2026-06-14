using Back.Application.DTOs.Turma;
using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Turma;

public class GetTurmaByIdUseCase
{
    private readonly ITurmaRepository _repo;

    public GetTurmaByIdUseCase(ITurmaRepository repo)
    {
        _repo = repo;
    }

    public async Task<TurmaResponse> ExecuteAsync(string identifier)
    {
        var turma = await _repo.GetByIdentifierAsync(identifier);
        if (turma == null)
            throw new KeyNotFoundException("Turma não encontrada.");

        return new TurmaResponse(
            turma.Id,
            turma.Periodo!,
            turma.Turno!,
            turma.Codigo!,
            turma.CodigoAtivo,
            turma.PossuiExtensao,
            turma.MaximoHorasExtensao,
            turma.CursoId,
            turma.Curso?.Nome ?? "curso não encontrado",
            turma.Alunos.Count);
    }
}
