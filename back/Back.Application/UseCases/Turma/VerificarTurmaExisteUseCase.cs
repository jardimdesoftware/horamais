using Back.Application.DTOs.Turma;
using Back.Application.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Turma;

public class VerificarTurmaExisteUseCase
{
    private readonly ITurmaRepository _repo;

    public VerificarTurmaExisteUseCase(ITurmaRepository repo)
    {
        _repo = repo;
    }

    public async Task<VerificarTurmaResponse?> ExecuteAsync(string codigo)
    {
        var turma = await _repo.GetByCodigoAsync(codigo);
        if (turma == null || !turma.CodigoAtivo) return null;

        return new VerificarTurmaResponse(
            turma.Periodo!,
            turma.Curso?.Nome ?? "Curso não encontrado"
        );
    }
}
