using Back.Application.DTOs.Aluno;
using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Aluno;

public class GetAlunoDetalhadoUseCase
{
    private readonly IAlunoRepository _alunoRepo;
    private readonly IAlunoAtividadeRepository _atividadeRepo;
    private readonly IAtividadeRepository _atividadeCursoRepo;
    private readonly ILimiteHorasAlunoRepository _limiteRepo;

    public GetAlunoDetalhadoUseCase(
        IAlunoRepository alunoRepo,
        IAlunoAtividadeRepository atividadeRepo,
        ILimiteHorasAlunoRepository limiteRepo,
        IAtividadeRepository atividadeCursoRepo)
    {
        _alunoRepo = alunoRepo;
        _atividadeRepo = atividadeRepo;
        _limiteRepo = limiteRepo;
        _atividadeCursoRepo = atividadeCursoRepo;
    }

    public async Task<AlunoDetalhadoResponse> ExecuteAsync(Guid alunoId)
    {
        var aluno = await _alunoRepo.GetByIdWithAtividadesAsync(alunoId);
        if (aluno == null)
            throw new KeyNotFoundException("Aluno não encontrado");

        // Atividades são globais: busca todas e faz "left join" com as do aluno,
        // para garantir que o aluno veja todas as atividades disponíveis (mesmo com 0 horas).
        var atividadesCurso = await _atividadeCursoRepo.GetAllAsync();

        var atividadesPorAluno = aluno.Atividades
            .ToDictionary(a => a.AtividadeId, a => a);

        var atividades = atividadesCurso.Select(atividade =>
        {
            atividadesPorAluno.TryGetValue(atividade.Id, out var alunoAtividade);
            var horasConcluidas = alunoAtividade?.HorasConcluidas ?? 0;

            return new AtividadeAlunoResumo(
                atividade.Id,
                atividade.Nome!,
                atividade.Grupo!,
                atividade.Categoria!,
                atividade.CategoriaKey!,
                atividade.CargaMaximaSemestral,
                atividade.CargaMaximaCurso,
                horasConcluidas,
                atividade.Tipo.ToString()
            );
        });

        var limite = await _limiteRepo.GetByCursoIdAsync(aluno.Turma!.CursoId);

        var totalHorasExtensao = aluno.Atividades
            .Where(x => x.Atividade!.Tipo == Domain.Entities.Atividade.TipoAtividade.EXTENSAO)
            .Sum(x => x.HorasConcluidas);

        var totalHorasComplementar = aluno.Atividades
            .Where(x => x.Atividade!.Tipo == Domain.Entities.Atividade.TipoAtividade.COMPLEMENTAR)
            .Sum(x => x.HorasConcluidas);

        return new AlunoDetalhadoResponse(
            aluno.Id,
            aluno.Nome!,
            aluno.Email!,
            aluno.Matricula!,
            aluno.IsAtivo,
            aluno.TurmaId,
            atividades,
            totalHorasExtensao,
            aluno.Turma!.MaximoHorasExtensao ?? 0,
            totalHorasComplementar,
            limite != null && limite.MaximoHorasComplementar > 0 ? limite.MaximoHorasComplementar : 120
        );
    }
}
