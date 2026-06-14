using System;
using System.Threading.Tasks;
using Back.Application.Interfaces.Repositories;

namespace Back.Application.UseCases.Aluno;

public class ToggleAlunoStatusUseCase
{
    private readonly IAlunoRepository _repo;

    public ToggleAlunoStatusUseCase(IAlunoRepository repo)
    {
        _repo = repo;
    }

    public async Task ExecuteAsync(Guid id)
    {
        var aluno = await _repo.GetByIdAsync(id);
        if (aluno == null) throw new InvalidOperationException("Aluno não encontrado");

        aluno.IsAtivo = !aluno.IsAtivo;
        await _repo.UpdateAsync(aluno);
    }
}