using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.UseCases.LembreteEmail;

public class RemoverLembreteEmailUseCase
{
    private readonly ILembreteEmailRepository _repo;

    public RemoverLembreteEmailUseCase(ILembreteEmailRepository repo)
    {
        _repo = repo;
    }

    public async Task ExecuteAsync(Guid cursoId, Guid id)
    {
        var lembrete = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Lembrete não encontrado.");

        if (lembrete.CursoId != cursoId)
            throw new UnauthorizedAccessException("Este lembrete não pertence ao seu curso.");

        await _repo.DeleteAsync(lembrete);
        await _repo.SaveChangesAsync();
    }
}
