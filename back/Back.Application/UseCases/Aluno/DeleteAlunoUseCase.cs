using Back.Application.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Aluno;

public class DeleteAlunoUseCase
{
    private readonly IAlunoRepository _repo;

    public DeleteAlunoUseCase(IAlunoRepository repo)
    {
        _repo = repo;
    }

    public async Task ExecuteAsync(Guid id)
    {
        await _repo.DeleteComIdentityAsync(id);
    }
}
