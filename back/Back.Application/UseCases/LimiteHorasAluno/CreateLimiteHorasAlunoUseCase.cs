using Back.Application.DTOs.LimiteHorasAluno;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.LimiteHorasAluno;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.LimiteHorasAluno;

public class CreateLimiteHorasAlunoUseCase
{
    private readonly ILimiteHorasAlunoRepository _repo;

    public CreateLimiteHorasAlunoUseCase(ILimiteHorasAlunoRepository repo)
    {
        _repo = repo;
    }

    public async Task<Guid> ExecuteAsync(CreateLimiteHorasAlunoRequest request)
    {
        var limite = new LimiteHorasAlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithMaximoHorasComplementar(request.MaximoHorasComplementar)
            .WithCursoId(request.CursoId)
            .Build();

        await _repo.AddAsync(limite);
        return limite.Id;
    }
}
