using Back.Application.DTOs.Aluno;
using Back.Application.Interfaces.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Aluno;

public class GetResumoHorasUseCase
{
    private readonly IAlunoRepository _alunoRepo;
    private readonly ILimiteHorasAlunoRepository _limiteRepo;

    public GetResumoHorasUseCase(IAlunoRepository alunoRepo, ILimiteHorasAlunoRepository limiteRepo)
    {
        _alunoRepo = alunoRepo;
        _limiteRepo = limiteRepo;
    }

    public async Task<IEnumerable<AlunoResumoHorasResponse>> ExecuteAsync()
    {
        var alunos = await _alunoRepo.GetAllWithAtividadesAsync();

        return alunos.Select(a =>
        {
            var limite = _limiteRepo.GetByCursoIdAsync(a.Turma!.CursoId).Result;

            var ext = a.Atividades
                .Where(x => x.Atividade!.Tipo == Domain.Entities.Atividade.TipoAtividade.EXTENSAO)
                .Sum(x => x.HorasConcluidas);

            var comp = a.Atividades
                .Where(x => x.Atividade!.Tipo == Domain.Entities.Atividade.TipoAtividade.COMPLEMENTAR)
                .Sum(x => x.HorasConcluidas);

            return new AlunoResumoHorasResponse(
                a.Id,
                a.Nome!,
                a.Email!,
                ext,
                a.Turma!.MaximoHorasExtensao ?? 0,
                comp,
                limite?.MaximoHorasComplementar ?? 0,
                a.IsAtivo
            );
        });
    }

}