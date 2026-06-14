using Back.Application.DTOs.Certificado;
using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Certificado;
public class GetCertificadosByCursoIdUseCase
{
    private readonly ICertificadoRepository _certificadoRepo;
    public GetCertificadosByCursoIdUseCase(
        ICertificadoRepository certificadoRepo)
    {
        _certificadoRepo = certificadoRepo;
    }

    public async Task<IEnumerable<CertificadoPorCursoResponse>> ExecuteAsync(Guid cursoId)
    {
        var certificados = await _certificadoRepo.GetAllWithAlunoAtividadeAsync();

        return certificados
            .Where(c => c.AlunoAtividade?.Aluno?.Turma?.CursoId == cursoId)
            .Select(c => new CertificadoPorCursoResponse(
                c.Id,
                c.Grupo!,
                c.Categoria!,
                c.TituloAtividade!,
                c.CargaHoraria,
                c.Local!,
                c.DataInicio,
                c.DataFim,
                c.Status.ToString(),
                c.Tipo.ToString(),
                c.AlunoAtividade!.Aluno!.Id,
                c.AlunoAtividade.Aluno.Nome!,
                c.AlunoAtividade.Aluno.Email!,
                c.AlunoAtividade.Aluno.Matricula!,
                c.AlunoAtividade.Aluno.Turma!.Periodo!,
                c.JustificativaRejeicao,
                c.CargaHorariaOriginal,
                c.CargaHorariaCorrigida
            ));
    }
}
