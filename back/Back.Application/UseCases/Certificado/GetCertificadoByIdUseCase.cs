using Back.Application.DTOs.Certificado;
using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Certificado;

public class GetCertificadoByIdUseCase
{
    private readonly ICertificadoRepository _repo;

    public GetCertificadoByIdUseCase(ICertificadoRepository repo)
    {
        _repo = repo;
    }

    public async Task<CertificadoDetalhadoResponse> ExecuteAsync(Guid id)
    {
        var cert = await _repo.GetByIdWithAlunoAtividadeAsync(id);
        if (cert is null || cert.AlunoAtividade is null)
            throw new KeyNotFoundException("Certificado não encontrado.");

        return new CertificadoDetalhadoResponse(
            cert.Id,
            cert.TituloAtividade!,
            cert.Instituicao!,
            cert.Local!,
            cert.Categoria!,
            cert.Grupo!,
            cert.PeriodoLetivo!,
            cert.CargaHoraria,
            cert.DataInicio,
            cert.DataFim,
            cert.TotalPeriodos,
            cert.Descricao,
            cert.Tipo,
            cert.Status,
            cert.AlunoAtividade.AlunoId,
            cert.AlunoAtividade.AtividadeId,
            cert.JustificativaRejeicao,
            cert.CargaHorariaOriginal,
            cert.CargaHorariaCorrigida
        );
    }
}
