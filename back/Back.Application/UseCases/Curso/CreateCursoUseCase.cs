using Back.Application.DTOs.Curso;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Curso;
using Back.Domain.Entities.LimiteHorasAluno;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Curso;

public class CreateCursoUseCase
{
    private readonly ICursoRepository _cursoRepository;
    private readonly ILimiteHorasAlunoRepository _limiteHorasAlunoRepository;
    private readonly ICampusRepository _campusRepository;

    public CreateCursoUseCase(
        ICursoRepository cursoRepository,
        ILimiteHorasAlunoRepository limiteHorasAlunoRepository,
        ICampusRepository campusRepository)
    {
        _cursoRepository = cursoRepository;
        _limiteHorasAlunoRepository = limiteHorasAlunoRepository;
        _campusRepository = campusRepository;
    }

    public async Task<CursoResponse> ExecuteAsync(CreateCursoComLimiteHorasRequest request)
    {
        var campus = await _campusRepository.GetByIdAsync(request.CampusId);
        if (campus == null)
            throw new InvalidOperationException("Campus não encontrado.");

        var curso = new CursoBuilder()
            .WithId(Guid.NewGuid())
            .WithNome(request.NomeCurso!)
            .WithCampusId(request.CampusId)
            .Build();

        await _cursoRepository.AddAsync(curso);

        var limite = new LimiteHorasAlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithMaximoHorasComplementar(request.MaximoHorasComplementar)
            .WithCursoId(curso.Id)
            .Build();

        await _limiteHorasAlunoRepository.AddAsync(limite);

        return new CursoResponse(curso.Id, curso.Nome!);
    }
}
