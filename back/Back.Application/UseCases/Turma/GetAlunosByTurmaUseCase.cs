using Back.Application.DTOs.Aluno;
using Back.Application.DTOs.Turma;
using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Turma;

public class GetAlunosByTurmaUseCase
{
    private readonly ITurmaRepository _turmaRepo;
    private readonly IAlunoRepository _alunoRepo;
    private readonly ILimiteHorasAlunoRepository _limiteRepo;

    public GetAlunosByTurmaUseCase(
        ITurmaRepository turmaRepo,
        IAlunoRepository alunoRepo,
        ILimiteHorasAlunoRepository limiteRepo)
    {
        _turmaRepo = turmaRepo;
        _alunoRepo = alunoRepo;
        _limiteRepo = limiteRepo;
    }

    public async Task<IEnumerable<AlunoPorTurmaDetalhadoResponse>> ExecuteAsync(string identifier)
    {
        var turma = await _turmaRepo.GetByIdentifierAsync(identifier);
        if (turma == null)
            throw new KeyNotFoundException("Turma não encontrada.");

        var alunos = await _turmaRepo.GetAlunosByTurmaAsync(turma.Id);
        var limite = await _limiteRepo.GetByCursoIdAsync(turma.CursoId);

        var result = new List<AlunoPorTurmaDetalhadoResponse>();

        foreach (var aluno in alunos)
        {
            var atividades = await _alunoRepo.GetAtividadesByAlunoIdAsync(aluno.Id);

            var atividadesExtensao = atividades.Where(x => x.Atividade!.Tipo == Domain.Entities.Atividade.TipoAtividade.EXTENSAO);
            var atividadesComplementar = atividades.Where(x => x.Atividade!.Tipo == Domain.Entities.Atividade.TipoAtividade.COMPLEMENTAR);

            int totalHorasExtensao = atividadesExtensao.Sum(x => x.HorasConcluidas);
            int totalHorasComplementar = atividadesComplementar.Sum(x => x.HorasConcluidas);

            double porcentagem = 0;
            if (limite != null)
            {
                if (turma.PossuiExtensao)
                {
                    var pExt = turma.MaximoHorasExtensao > 0 ? ((double)totalHorasExtensao / turma.MaximoHorasExtensao!.Value) * 100 : 0;
                    var pComp = limite.MaximoHorasComplementar > 0 ? ((double)totalHorasComplementar / limite.MaximoHorasComplementar) * 100 : 0;
                    porcentagem = Math.Round((pExt + pComp) / 2, 2);
                }
                else
                {
                    porcentagem = limite.MaximoHorasComplementar > 0
                        ? Math.Round((double)totalHorasComplementar / limite.MaximoHorasComplementar * 100, 2)
                        : 0;
                }
            }

            result.Add(new AlunoPorTurmaDetalhadoResponse(
                aluno.Id,
                aluno.Nome!,
                aluno.Email!,
                aluno.Matricula!,
                aluno.IsAtivo,
                totalHorasExtensao,
                totalHorasComplementar,
                turma.MaximoHorasExtensao ?? 0,
                limite?.MaximoHorasComplementar ?? 0,
                porcentagem
            ));
        }

        return result;
    }
}
