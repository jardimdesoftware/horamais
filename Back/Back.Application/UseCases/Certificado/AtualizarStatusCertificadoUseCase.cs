using System;
using System.Linq;
using System.Threading.Tasks;
using Back.Application.Interfaces;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Atividade;
using Back.Domain.Entities.Certificado;

namespace Back.Application.UseCases.Certificado;

public class AtualizarStatusCertificadoUseCase
{
    private readonly ICertificadoRepository _repo;
    private readonly IAlunoAtividadeRepository _alunoAtividadeRepo;
    private readonly ILimiteHorasAlunoRepository _limiteRepo;
    private readonly INotificarSecretariaConclusaoUseCase _notificarSecretaria;

    public AtualizarStatusCertificadoUseCase(
        ICertificadoRepository repo,
        IAlunoAtividadeRepository alunoAtividadeRepo,
        ILimiteHorasAlunoRepository limiteRepo,
        INotificarSecretariaConclusaoUseCase notificarSecretaria)
    {
        _repo = repo;
        _alunoAtividadeRepo = alunoAtividadeRepo;
        _limiteRepo = limiteRepo;
        _notificarSecretaria = notificarSecretaria;
    }

    public async Task<bool> ExecuteAsync(Guid id, StatusCertificado novoStatus, string? justificativaRejeicao = null, int? novaCargaHoraria = null)
    {
        if (novoStatus == StatusCertificado.REPROVADO && string.IsNullOrWhiteSpace(justificativaRejeicao))
            throw new ArgumentException("A justificativa é obrigatória ao reprovar um certificado.");

        var certificado = await _repo.GetByIdWithAlunoAtividadeAsync(id);
        if (certificado == null) return false;

        var alunoAtividade = certificado.AlunoAtividade!;
        var atividade = alunoAtividade.Atividade!;
        var cursoId = alunoAtividade.Aluno!.Turma!.CursoId;
        var limite = await _limiteRepo.GetByCursoIdAsync(cursoId);

        if (novoStatus == StatusCertificado.APROVADO
            && certificado.Status != StatusCertificado.APROVADO
            && novaCargaHoraria.HasValue
            && novaCargaHoraria.Value > 0
            && novaCargaHoraria.Value != certificado.CargaHoraria)
        {
            certificado.CargaHorariaOriginal = certificado.CargaHoraria;
            certificado.CargaHoraria = novaCargaHoraria.Value;
            certificado.CargaHorariaCorrigida = true;
        }

        if (novoStatus == StatusCertificado.APROVADO && certificado.Status != StatusCertificado.APROVADO)
        {
            int maxTipo = atividade.Tipo == TipoAtividade.EXTENSAO
                ? alunoAtividade.Aluno!.Turma!.MaximoHorasExtensao ?? int.MaxValue
                : limite?.MaximoHorasComplementar ?? int.MaxValue;

            int horasTotaisAcumuladasDoTipo = await _alunoAtividadeRepo.GetTotalHorasConcluidasPorTipoAsync(alunoAtividade.AlunoId, atividade.Tipo);

            int restanteTipo = Math.Max(0, maxTipo - horasTotaisAcumuladasDoTipo);

            var certificadosAprovados = (await _repo.GetByAlunoAtividadeAsync(certificado.AlunoAtividadeId))
                .Where(c => c.Status == StatusCertificado.APROVADO && c.Id != certificado.Id)
                .ToList();

            if (certificado.TotalPeriodos <= 0)
                throw new ArgumentException(
                    $"Certificado '{certificado.Id}' possui TotalPeriodos inválido ({certificado.TotalPeriodos}).");

            int horasPorPeriodo = certificado.CargaHoraria / certificado.TotalPeriodos;
            int totalPermitido = 0;

            for (int i = 0; i < certificado.TotalPeriodos; i++)
            {
                var periodoAtual = AvançarPeriodo(certificado.PeriodoLetivo, i);

                var horasMesmoPeriodo = certificadosAprovados
                    .Where(c => c.Grupo == certificado.Grupo && c.PeriodoLetivo == periodoAtual)
                    .Sum(c => c.CargaHoraria / c.TotalPeriodos);

                int restanteSemestre = Math.Max(0, atividade.CargaMaximaSemestral - horasMesmoPeriodo);
                int restanteCurso = Math.Max(0, atividade.CargaMaximaCurso - alunoAtividade.HorasConcluidas - totalPermitido);

                // O valor permitido é o menor entre:
                // - A carga horária do período atual do certificado
                // - O limite restante do semestre para a atividade
                // - O limite restante do curso para a atividade
                // - O limite GLOBAL restante do TIPO para o ALUNO (a correção principal)
                int permitidoNestePeriodo = Math.Min(
                    horasPorPeriodo,
                    Math.Min(restanteSemestre, Math.Min(restanteCurso, restanteTipo))
                );


                if (permitidoNestePeriodo <= 0)
                    break;

                totalPermitido += permitidoNestePeriodo;
                restanteTipo -= permitidoNestePeriodo;
            }

            if (totalPermitido > 0)
            {
                alunoAtividade.HorasConcluidas += totalPermitido;
                await _alunoAtividadeRepo.UpdateAsync(alunoAtividade);
            }
        }

        certificado.Status = novoStatus;
        certificado.JustificativaRejeicao = novoStatus == StatusCertificado.REPROVADO
            ? justificativaRejeicao
            : null;
        await _repo.UpdateAsync(certificado);

        // Ao aprovar, verifica se o aluno atingiu a carga exigida do tipo e, em
        // caso afirmativo, notifica a secretaria (uma única vez por aluno e tipo).
        if (novoStatus == StatusCertificado.APROVADO)
            await _notificarSecretaria.ExecuteAsync(alunoAtividade.AlunoId, atividade.Tipo);

        return true;
    }

    private static string AvançarPeriodo(string? periodo, int passos)
    {
        if (string.IsNullOrWhiteSpace(periodo))
            throw new ArgumentException("Período letivo não pode ser nulo ou vazio.");

        var partes = periodo.Split('.');

        if (partes.Length != 2
            || !int.TryParse(partes[0], out int ano)
            || !int.TryParse(partes[1], out int semestre)
            || semestre < 1 || semestre > 2)
        {
            throw new ArgumentException(
                $"Período letivo '{periodo}' inválido. Use o formato YYYY.1 ou YYYY.2.");
        }

        semestre += passos;
        ano += (semestre - 1) / 2;
        semestre = ((semestre - 1) % 2) + 1;
        return $"{ano}.{semestre}";
    }
}
