using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Certificado;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Certificado;

public class ValidarLimiteCertificadoUseCase
{
    private readonly ICertificadoRepository _certificadoRepo;

    private static readonly StatusCertificado[] StatusesAtivos =
        [StatusCertificado.PENDENTE, StatusCertificado.APROVADO];

    public ValidarLimiteCertificadoUseCase(ICertificadoRepository certificadoRepo)
    {
        _certificadoRepo = certificadoRepo;
    }

    public async Task ExecuteAsync(
        Guid alunoAtividadeId,
        int cargaHoraria,
        string periodoLetivo,
        int cargaMaximaSemestral,
        int cargaMaximaCurso,
        Guid? ignorarCertificadoId = null)
    {
        var existentes = await _certificadoRepo
            .GetByAlunoAtividadeAndStatusAsync(alunoAtividadeId, StatusesAtivos);

        if (ignorarCertificadoId is { } idIgnorar)
            existentes = existentes.Where(c => c.Id != idIgnorar);

        var somaSemestral = existentes
            .Where(c => c.PeriodoLetivo == periodoLetivo)
            .Sum(c => c.CargaHoraria);

        if (somaSemestral + cargaHoraria > cargaMaximaSemestral)
            throw new InvalidOperationException(
                $"Limite semestral de {cargaMaximaSemestral}h para esta atividade foi atingido. " +
                $"Você já possui {somaSemestral}h registradas neste período.");

        var somaCurso = existentes.Sum(c => c.CargaHoraria);

        if (somaCurso + cargaHoraria > cargaMaximaCurso)
            throw new InvalidOperationException(
                $"Limite de {cargaMaximaCurso}h no curso para esta atividade foi atingido. " +
                $"Você já possui {somaCurso}h registradas.");
    }
}
