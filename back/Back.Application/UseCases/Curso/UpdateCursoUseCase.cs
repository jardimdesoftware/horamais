using Back.Application.DTOs.Curso;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.LimiteHorasAluno;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Curso;

public class UpdateCursoUseCase
{
    private readonly ICursoRepository _cursoRepository;
    private readonly ILimiteHorasAlunoRepository _limiteHorasRepository;
    private readonly ICampusRepository _campusRepository;

    public UpdateCursoUseCase(
        ICursoRepository cursoRepository,
        ILimiteHorasAlunoRepository limiteHorasRepository,
        ICampusRepository campusRepository)
    {
        _cursoRepository = cursoRepository;
        _limiteHorasRepository = limiteHorasRepository;
        _campusRepository = campusRepository;
    }

    public async Task ExecuteAsync(Guid id, UpdateCursoComLimiteHorasRequest request)
    {
        var curso = await _cursoRepository.GetByIdToUpdateAsync(id);
        if (curso == null)
            throw new KeyNotFoundException("Curso não encontrado.");

        var campus = await _campusRepository.GetByIdAsync(request.CampusId);
        if (campus == null)
            throw new InvalidOperationException("Campus não encontrado.");

        curso.Nome = request.NomeCurso;
        curso.CampusId = request.CampusId;
        await _cursoRepository.UpdateAsync(curso);

        var limite = await _limiteHorasRepository.GetByCursoIdToUpdateAsync(id);
        if (limite == null)
        {
            var novoLimite = new LimiteHorasAlunoBuilder()
                .WithId(Guid.NewGuid())
                .WithCursoId(id)
                .WithMaximoHorasComplementar(request.MaximoHorasComplementar)
                .Build();
            await _limiteHorasRepository.AddAsync(novoLimite);
        }
        else
        {
            limite.MaximoHorasComplementar = request.MaximoHorasComplementar;
            await _limiteHorasRepository.UpdateAsync(limite);
        }
    }
}
