using Back.Application.DTOs.Aluno;
using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Aluno;

public class GetAlunoByIdUseCase
{
    private readonly IAlunoRepository _repo;

    public GetAlunoByIdUseCase(IAlunoRepository repo)
    {
        _repo = repo;
    }

    public async Task<AlunoResponse> ExecuteAsync(Guid id)
    {
        var aluno = await _repo.GetByIdAsync(id);

        if (aluno == null)
            throw new KeyNotFoundException("Aluno não encontrado.");

        return new AlunoResponse(aluno.Id, aluno.Nome!, aluno.Email!, aluno.Matricula!, "ALUNO");
    }
}
