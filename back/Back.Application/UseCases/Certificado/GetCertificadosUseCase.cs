using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Back.Application.DTOs.Certificado;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Certificado;
using System.Linq;

namespace Back.Application.UseCases.Certificado;

public class GetCertificadosUseCase
{
    private readonly ICertificadoRepository _repo;

    public GetCertificadosUseCase(ICertificadoRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<CertificadoResponse>> ExecuteAsync(StatusCertificado? status, Guid? alunoId)
    {
        var certificados = await _repo.GetAsync(status, alunoId);

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
            c.AlunoAtividade.Atividade!.CategoriaKey!,
            c.JustificativaRejeicao,
            c.CargaHorariaOriginal,
            c.CargaHorariaCorrigida
        ));
    }
}
