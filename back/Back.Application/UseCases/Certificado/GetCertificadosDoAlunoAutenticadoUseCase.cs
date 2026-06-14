using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Back.Application.DTOs.Certificado;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Certificado;
using System.Linq;

namespace Back.Application.UseCases.Certificado;

public class GetCertificadosDoAlunoAutenticadoUseCase
{
    private readonly ICertificadoRepository _repo;
    private readonly IAlunoRepository _alunoRepo;

    public GetCertificadosDoAlunoAutenticadoUseCase(
        ICertificadoRepository repo,
        IAlunoRepository alunoRepo)
    {
        _repo = repo;
        _alunoRepo = alunoRepo;
    }

    public async Task<IEnumerable<CertificadoResponse>> ExecuteAsync(string identityUserId)
    {
        var aluno = await _alunoRepo.GetByIdentityUserIdAsync(identityUserId);
        if (aluno == null)
            throw new InvalidOperationException("Aluno não encontrado para este usuário.");

        var certificados = await _repo.GetAsync(null, aluno.Id);

        return certificados.Select(c => new CertificadoResponse(
            c.Id,
            c.TituloAtividade!,
            c.Instituicao!,
            c.Local!,
            c.Categoria!,
            c.Grupo!,
            c.PeriodoLetivo!,
            c.CargaHoraria,
            c.DataInicio,
            c.DataFim,
            c.TotalPeriodos,
            c.Descricao,
            c.Tipo,
            c.Status,
            c.AlunoAtividade!.AlunoId,
            c.AlunoAtividade.AtividadeId,
            c.AlunoAtividade.Atividade.CategoriaKey!,
            c.JustificativaRejeicao,
            c.CargaHorariaOriginal,
            c.CargaHorariaCorrigida
        ));
    }
}
