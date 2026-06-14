using Back.Application.DTOs.Curso;
using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Curso;

public class GetCursoByIdUseCase
{
    private readonly ICursoRepository _repository;
    private readonly ILimiteHorasAlunoRepository _limiteRepository;

    public GetCursoByIdUseCase(ICursoRepository repository, ILimiteHorasAlunoRepository limiteRepository)
    {
        _repository = repository;
        _limiteRepository = limiteRepository;
    }

    public async Task<CursoDetalhadoResponse> ExecuteAsync(Guid id)
    {
        var curso = await _repository.GetByIdAsync(id);
        if (curso == null)
            throw new KeyNotFoundException("Curso não encontrado.");

        var limite = await _limiteRepository.GetByCursoIdAsync(id);

        return new CursoDetalhadoResponse(
            curso.Id,
            curso.Nome,
            limite?.MaximoHorasComplementar ?? 0,
            curso.CampusId,
            curso.Campus?.Nome ?? string.Empty
        );
    }
}
