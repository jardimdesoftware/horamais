using Back.Application.DTOs.Turma;
using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Turma;

public class UpdateTurmaUseCase
{
    private readonly ITurmaRepository _repo;

    public UpdateTurmaUseCase(ITurmaRepository repo)
    {
        _repo = repo;
    }

    public async Task<TurmaResponse> ExecuteAsync(string identifier, UpdateTurmaRequest request)
    {
        var periodo = TurmaInputValidator.ValidarPeriodo(request.Periodo);
        var turno = TurmaInputValidator.NormalizarTurno(request.Turno);

        // 1. Busca a turma (rastreada e com includes)
        var turma = await _repo.GetByIdentifierTrackedAsync(identifier);
        if (turma == null)
            throw new KeyNotFoundException("Turma não encontrada.");

        if (await _repo.ExistsByCursoPeriodoTurnoAsync(request.CursoId, periodo, turno, turma.Id))
            throw new InvalidOperationException("Já existe uma turma para este curso, período e turno.");

        // 2. Atualiza as propriedades
        turma.Periodo = periodo;
        turma.Turno = turno;
        turma.PossuiExtensao = request.PossuiExtensao;
        turma.MaximoHorasExtensao = request.MaximoHorasExtensao;
        turma.CursoId = request.CursoId;

        // 3. Salva
        await _repo.UpdateAsync(turma);

        // 4. Retorna a resposta formatada
        return new TurmaResponse(
            turma.Id,
            turma.Periodo!,
            turma.Turno!,
            turma.Codigo!,
            turma.CodigoAtivo,
            turma.PossuiExtensao,
            turma.MaximoHorasExtensao,
            turma.CursoId,
            turma.Curso?.Nome ?? "Curso não encontrado",
            turma.Alunos.Count
        );
    }
}