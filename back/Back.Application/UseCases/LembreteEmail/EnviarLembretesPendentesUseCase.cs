using Back.Application.Interfaces;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Atividade;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Application.UseCases.LembreteEmail;

/// <summary>
/// Dispara os lembretes cuja data já chegou (e ainda não foram enviados) para os
/// alunos pendentes — ativos e que ainda não concluíram suas horas complementares.
/// É idempotente: cada lembrete é marcado como enviado após o disparo.
/// </summary>
public class EnviarLembretesPendentesUseCase
{
    public const string MensagemPadrao =
        "Este é um lembrete para que você envie seus certificados de horas complementares. " +
        "Ainda há pendências registradas no seu nome — não deixe para o fim do semestre.";

    private const string Assunto = "Lembrete: horas complementares pendentes — hora+";

    private readonly ILembreteEmailRepository _lembreteRepo;
    private readonly IAlunoRepository _alunoRepo;
    private readonly ILimiteHorasAlunoRepository _limiteRepo;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;

    public EnviarLembretesPendentesUseCase(
        ILembreteEmailRepository lembreteRepo,
        IAlunoRepository alunoRepo,
        ILimiteHorasAlunoRepository limiteRepo,
        IEmailService emailService,
        IEmailTemplateService templateService)
    {
        _lembreteRepo = lembreteRepo;
        _alunoRepo = alunoRepo;
        _limiteRepo = limiteRepo;
        _emailService = emailService;
        _templateService = templateService;
    }

    /// <returns>Quantidade de e-mails enviados com sucesso.</returns>
    public async Task<int> ExecuteAsync()
    {
        var lembretes = (await _lembreteRepo.GetPendentesAteAsync(DateTime.UtcNow.Date)).ToList();
        if (lembretes.Count == 0)
            return 0;

        var limites = (await _limiteRepo.GetAllAsync()).ToDictionary(l => l.CursoId);
        var enviados = 0;

        foreach (var grupo in lembretes.GroupBy(l => l.CursoId))
        {
            var cursoId = grupo.Key;
            limites.TryGetValue(cursoId, out var limite);

            var pendentes = (await _alunoRepo.GetAlunosPorCursoComDetalhesAsync(cursoId))
                .Where(a => a.IsAtivo && !string.IsNullOrWhiteSpace(a.Email))
                .Where(a => EstaPendente(a, limite?.MaximoHorasComplementar ?? 0))
                .ToList();

            foreach (var lembrete in grupo)
            {
                var mensagem = string.IsNullOrWhiteSpace(lembrete.MensagemPersonalizada)
                    ? MensagemPadrao
                    : lembrete.MensagemPersonalizada!;

                foreach (var aluno in pendentes)
                {
                    var corpo = _templateService.RenderLembretePendencia(aluno.Nome ?? "aluno", mensagem);
                    try
                    {
                        await _emailService.EnviarEmailAsync(aluno.Email!, Assunto, corpo);
                        enviados++;
                    }
                    catch
                    {
                        // Falha no envio para um aluno não deve interromper os demais.
                    }
                }

                lembrete.Enviado = true;
                lembrete.EnviadoEm = DateTime.UtcNow;
                await _lembreteRepo.UpdateAsync(lembrete);
            }
        }

        await _lembreteRepo.SaveChangesAsync();
        return enviados;
    }

    /// <summary>
    /// Aluno é considerado pendente quando ainda não atingiu o máximo de horas
    /// complementares do curso. Sem limite configurado, todos os ativos são pendentes.
    /// </summary>
    private static bool EstaPendente(Domain.Entities.Aluno.Aluno aluno, int maximoHorasComplementar)
    {
        if (maximoHorasComplementar <= 0)
            return true;

        var totalComplementar = aluno.Atividades
            .Where(x => x.Atividade?.Tipo == TipoAtividade.COMPLEMENTAR)
            .Sum(x => x.HorasConcluidas);

        return totalComplementar < maximoHorasComplementar;
    }
}
