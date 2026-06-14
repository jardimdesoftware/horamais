using Back.Application.DTOs.Aluno;
using Back.Application.Interfaces.Repositories;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Aluno;
public class GetAlunoFromTokenUseCase
{
    private readonly IAlunoRepository _alunoRepo;
    private readonly ILimiteHorasAlunoRepository _limiteRepo;
    public GetAlunoFromTokenUseCase(
        IAlunoRepository alunoRepo,
        ILimiteHorasAlunoRepository limiteRepo)
    {
        _alunoRepo = alunoRepo;
        _limiteRepo = limiteRepo;
    }

    public async Task<AlunoDetalhadoResponse> ExecuteAsync(ClaimsPrincipal user)
    {
        var identityUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Usuário não identificado.");

        var aluno = await _alunoRepo.GetByIdentityUserIdWithAtividadesAsync(identityUserId)
            ?? throw new UnauthorizedAccessException("Aluno não encontrado.");

        var limite = await _limiteRepo.GetByCursoIdAsync(aluno.Turma!.CursoId);

        var atividades = aluno.Atividades.Select(a => new AtividadeAlunoResumo(
            a.AtividadeId,
            a.Atividade!.Nome!,
            a.Atividade.Grupo!,
            a.Atividade.Categoria!,
            a.Atividade.CategoriaKey!,
            a.Atividade.CargaMaximaSemestral,
            a.Atividade.CargaMaximaCurso,
            a.HorasConcluidas,
            a.Atividade.Tipo.ToString()
        ));

        var ext = aluno.Atividades
            .Where(x => x.Atividade!.Tipo == Domain.Entities.Atividade.TipoAtividade.EXTENSAO)
            .Sum(x => x.HorasConcluidas);

        var comp = aluno.Atividades
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
            ext,
            aluno.Turma!.MaximoHorasExtensao ?? 0,
            comp,
            limite != null && limite.MaximoHorasComplementar > 0 ? limite.MaximoHorasComplementar : 120
        );
    }
}
