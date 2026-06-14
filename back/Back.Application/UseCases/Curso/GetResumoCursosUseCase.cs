using System.Collections.Generic;
using System.Threading.Tasks;
using Back.Application.DTOs.Curso;
using Back.Application.Interfaces.Repositories;

namespace Back.Application.UseCases.Curso;

public class GetResumoCursosUseCase
{
    private readonly ICursoRepository _repo;

    public GetResumoCursosUseCase(ICursoRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<CursoResumoResponse>> ExecuteAsync()
    {
        return await _repo.GetResumoCursosAsync();
    }
}
