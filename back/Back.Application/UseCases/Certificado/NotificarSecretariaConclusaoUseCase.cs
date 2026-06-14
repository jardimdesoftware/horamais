using Back.Application.Interfaces;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Atividade;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Certificado;

/// <summary>
/// Notifica a secretaria, uma única vez por aluno e por tipo de carga, quando o
/// aluno atinge a carga horária exigida. Extensão e complementar são avaliadas
/// de forma independente (PPC novo).
/// </summary>
public class NotificarSecretariaConclusaoUseCase : INotificarSecretariaConclusaoUseCase
{
    private readonly IAlunoRepository _alunoRepo;
    private readonly IAlunoAtividadeRepository _alunoAtividadeRepo;
    private readonly ILimiteHorasAlunoRepository _limiteRepo;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;
    private readonly IConfiguration _config;

    public NotificarSecretariaConclusaoUseCase(
        IAlunoRepository alunoRepo,
        IAlunoAtividadeRepository alunoAtividadeRepo,
        ILimiteHorasAlunoRepository limiteRepo,
        IEmailService emailService,
        IEmailTemplateService templateService,
        IConfiguration config)
    {
        _alunoRepo = alunoRepo;
        _alunoAtividadeRepo = alunoAtividadeRepo;
        _limiteRepo = limiteRepo;
        _emailService = emailService;
        _templateService = templateService;
        _config = config;
    }

    public async Task ExecuteAsync(Guid alunoId, TipoAtividade tipo)
    {
        var aluno = await _alunoRepo.GetByIdWithAtividadesAsync(alunoId);
        if (aluno?.Turma == null)
            return;

        // Evita reenvio: cada tipo é notificado uma única vez por aluno.
        var jaNotificado = tipo == TipoAtividade.EXTENSAO
            ? aluno.NotificadoSecretariaExtensao
            : aluno.NotificadoSecretariaComplementar;
        if (jaNotificado)
            return;

        var exigido = tipo == TipoAtividade.EXTENSAO
            ? aluno.Turma.MaximoHorasExtensao
            : (await _limiteRepo.GetByCursoIdAsync(aluno.Turma.CursoId))?.MaximoHorasComplementar;

        // Sem exigência definida para o tipo (ex.: PPC sem extensão) → nada a notificar.
        if (exigido is not > 0)
            return;

        var concluidas = await _alunoAtividadeRepo.GetTotalHorasConcluidasPorTipoAsync(alunoId, tipo);
        if (concluidas < exigido.Value)
            return;

        var secretaria = _config["SECRETARIA_EMAIL"];
        if (string.IsNullOrWhiteSpace(secretaria))
            return; // Destinatário não configurado: não interrompe a aprovação.

        var tipoLabel = tipo == TipoAtividade.EXTENSAO ? "Extensão" : "Complementar";
        var corpo = _templateService.RenderConclusaoCargaSecretaria(
            aluno.Nome ?? "Aluno",
            aluno.Matricula ?? "-",
            tipoLabel,
            concluidas,
            exigido.Value);

        await _emailService.EnviarEmailAsync(
            secretaria,
            $"Aluno concluiu a carga horária de {tipoLabel} — hora+",
            corpo);

        if (tipo == TipoAtividade.EXTENSAO)
            aluno.NotificadoSecretariaExtensao = true;
        else
            aluno.NotificadoSecretariaComplementar = true;

        await _alunoRepo.UpdateAsync(aluno);
    }
}
