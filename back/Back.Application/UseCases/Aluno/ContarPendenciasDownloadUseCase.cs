using Back.Application.DTOs.Aluno;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Atividade;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Aluno;

public class ContarPendenciasDownloadUseCase
{
    private readonly IAlunoRepository _alunoRepo;
    private readonly ILimiteHorasAlunoRepository _limiteRepo;
    private readonly ICoordenadorRepository _coordenadorRepo;

    public ContarPendenciasDownloadUseCase(
        IAlunoRepository alunoRepo,
        ILimiteHorasAlunoRepository limiteRepo,
        ICoordenadorRepository coordenadorRepo)
    {
        _alunoRepo = alunoRepo;
        _limiteRepo = limiteRepo;
        _coordenadorRepo = coordenadorRepo;
    }

    public async Task<ContagemPendenciaDownloadResponse> ExecuteAsync(ClaimsPrincipal user)
    {
        var identityUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Usuário não identificado no token.");

        var coordenador = await _coordenadorRepo.GetByIdentityUserIdWithCursoAsync(identityUserId)
            ?? throw new UnauthorizedAccessException("Coordenador não encontrado.");

        // CORREÇÃO 2: Verificar o objeto de navegação 'Curso' em vez do 'CursoId' para evitar o aviso.
        // Isso também é mais seguro.
        if (coordenador.Curso == null)
        {
            throw new InvalidOperationException("O coordenador não está associado a nenhum curso.");
        }
        var coordenadorCursoId = coordenador.Curso.Id; // Acessa o ID de forma segura

        var alunosDoCurso = await _alunoRepo.GetAlunosPorCursoComDetalhesAsync(coordenadorCursoId);
        var limiteDoCurso = await _limiteRepo.GetByCursoIdAsync(coordenadorCursoId);

        if (limiteDoCurso == null)
        {
            return new ContagemPendenciaDownloadResponse(0);
        }

        int contagemTotalDePendencias = 0;

        var horasExigidasComp = limiteDoCurso.MaximoHorasComplementar;

        foreach (var aluno in alunosDoCurso)
        {
            // --- Verificação para Horas COMPLEMENTARES ---
            if (horasExigidasComp > 0)
            {
                var horasConcluidasComp = aluno.Atividades
                    .Where(a => a.Atividade?.Tipo == TipoAtividade.COMPLEMENTAR)
                    .Sum(a => a.HorasConcluidas);

                if (horasConcluidasComp >= horasExigidasComp && !aluno.JaBaixadoHorasComplementares)
                {
                    contagemTotalDePendencias++;
                }
            }

            // --- Verificação para Horas de EXTENSÃO (por turma) ---
            var horasExigidasExt = aluno.Turma!.MaximoHorasExtensao;
            if (horasExigidasExt.HasValue && horasExigidasExt > 0)
            {
                var horasConcluidasExt = aluno.Atividades
                    .Where(a => a.Atividade?.Tipo == TipoAtividade.EXTENSAO)
                    .Sum(a => a.HorasConcluidas);

                if (horasConcluidasExt >= horasExigidasExt && !(aluno.JaBaixadoHorasExtensao ?? false))
                {
                    contagemTotalDePendencias++;
                }
            }
        }

        return new ContagemPendenciaDownloadResponse(contagemTotalDePendencias);
    }
}