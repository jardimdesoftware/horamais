using Back.Application.Interfaces.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Turma;

public class GetPeriodosLetivosUseCase
{
    private readonly ITurmaRepository _repo;

    public GetPeriodosLetivosUseCase(ITurmaRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<string>> ExecuteAsync()
        => await _repo.GetDistinctPeriodosAsync();
}
