using Back.Application.DTOs.Aluno;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Atividade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Aluno;

public class GetAlunosComHorasConcluidasUseCase
{
    private readonly IAlunoRepository _alunoRepo;
    private readonly ILimiteHorasAlunoRepository _limiteRepo;
    private readonly ICertificadoRepository _certificadoRepo;
    private readonly ICoordenadorRepository _coordenadorRepo;

    public GetAlunosComHorasConcluidasUseCase(
        IAlunoRepository alunoRepo,
        ILimiteHorasAlunoRepository limiteRepo,
        ICertificadoRepository certificadoRepo,
        ICoordenadorRepository coordenadorRepo)
    {
        _alunoRepo = alunoRepo;
        _limiteRepo = limiteRepo;
        _certificadoRepo = certificadoRepo;
        _coordenadorRepo = coordenadorRepo;
    }

    public async Task<IEnumerable<AlunoComHorasConcluidasResponse>> ExecuteAsync(TipoAtividade tipoAtividade, ClaimsPrincipal user)
    {
        var identityUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Usuário não identificado no token.");

        var coordenador = await _coordenadorRepo.GetByIdentityUserIdWithCursoAsync(identityUserId)
            ?? throw new UnauthorizedAccessException("Coordenador não encontrado.");

        if (coordenador.CursoId == null)
        {
            throw new InvalidOperationException("O coordenador não está associado a nenhum curso.");
        }
        var coordenadorCursoId = coordenador.CursoId;

        var alunosDoCurso = await _alunoRepo.GetAlunosPorCursoComDetalhesAsync(coordenadorCursoId);
        var limiteDoCurso = await _limiteRepo.GetByCursoIdAsync(coordenadorCursoId);

        if (limiteDoCurso == null)
        {
            return Enumerable.Empty<AlunoComHorasConcluidasResponse>();
        }

        var alunosConcluidos = new List<AlunoComHorasConcluidasResponse>();

        foreach (var aluno in alunosDoCurso)
        {
            int? horasExigidas = tipoAtividade == TipoAtividade.EXTENSAO
                ? aluno.Turma!.MaximoHorasExtensao
                : limiteDoCurso.MaximoHorasComplementar;

            if (horasExigidas == null || horasExigidas == 0) continue;

            // A variável 'horasConcluidas' já é calculada aqui
            var horasConcluidas = aluno.Atividades
                .Where(a => a.Atividade?.Tipo == tipoAtividade)
                .Sum(a => a.HorasConcluidas);

            if (horasConcluidas >= horasExigidas)
            {
                var certificadosAprovados = await _certificadoRepo.GetCertificadosAprovadosPorAlunoAsync(aluno.Id);

                // Lógica para determinar os novos campos
                var jaFezDownload = tipoAtividade == TipoAtividade.COMPLEMENTAR
                    ? aluno.JaBaixadoHorasComplementares
                    : aluno.JaBaixadoHorasExtensao ?? false; 

                var categoria = tipoAtividade == TipoAtividade.COMPLEMENTAR
                    ? "horasComplementares"
                    : "extensao";

                var alunoResponse = new AlunoComHorasConcluidasResponse(
                    Id: aluno.Id,
                    Nome: aluno.Nome,
                    Matricula: aluno.Matricula,
                    Email: aluno.Email,
                    Curso: aluno.Turma?.Curso?.Nome,
                    Certificados: certificadosAprovados.Select(c => new CertificadoConcluidoResponse(
                        Id: c.Id,
                        Grupo: c.Grupo,
                        Categoria: c.Categoria,
                        Titulo: c.TituloAtividade,
                        Descricao: c.Descricao,
                        CargaHoraria: c.CargaHoraria,
                        Local: c.Local,
                        PeriodoInicio: c.DataInicio.ToString("yyyy-MM-dd"),
                        PeriodoFim: c.DataFim.ToString("yyyy-MM-dd"),
                        Status: c.Status.ToString().ToLower(),
                        Tipo: c.Tipo.ToString().ToLower()
                    )).ToList(),
                    CargaHoraria: horasConcluidas,
                    CargaHorariaFinalizada: true, 
                    JaFezDownload: jaFezDownload,
                    Categoria: categoria
                );
                alunosConcluidos.Add(alunoResponse);
            }
        }
        return alunosConcluidos;
    }
}