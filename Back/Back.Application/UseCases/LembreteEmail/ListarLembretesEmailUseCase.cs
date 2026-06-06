using Back.Application.DTOs.LembreteEmail;
using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Application.UseCases.LembreteEmail;

public class ListarLembretesEmailUseCase
{
    private readonly ILembreteEmailRepository _repo;

    public ListarLembretesEmailUseCase(ILembreteEmailRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<LembreteEmailResponse>> ExecuteAsync(Guid cursoId)
    {
        var lembretes = await _repo.GetByCursoIdAsync(cursoId);
        return lembretes.Select(l => l.ToResponse());
    }
}
