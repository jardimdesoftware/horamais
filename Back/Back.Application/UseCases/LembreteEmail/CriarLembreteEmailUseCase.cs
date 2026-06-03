using Back.Application.DTOs.LembreteEmail;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.LembreteEmail;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.LembreteEmail;

public class CriarLembreteEmailUseCase
{
    public const int MaximoLembretesPorCurso = 3;

    private readonly ILembreteEmailRepository _repo;

    public CriarLembreteEmailUseCase(ILembreteEmailRepository repo)
    {
        _repo = repo;
    }

    public async Task<LembreteEmailResponse> ExecuteAsync(Guid cursoId, LembreteEmailRequest request)
    {
        var data = DateTime.SpecifyKind(request.Data.Date, DateTimeKind.Utc);

        if (data < DateTime.UtcNow.Date)
            throw new ArgumentException("Não é possível cadastrar uma data de lembrete no passado.");

        var total = await _repo.CountByCursoIdAsync(cursoId);
        if (total >= MaximoLembretesPorCurso)
            throw new InvalidOperationException(
                $"Limite de {MaximoLembretesPorCurso} datas de lembrete por semestre atingido.");

        var lembrete = new LembreteEmailBuilder()
            .WithId(Guid.NewGuid())
            .WithCursoId(cursoId)
            .WithData(data)
            .WithMensagemPersonalizada(string.IsNullOrWhiteSpace(request.Mensagem) ? null : request.Mensagem.Trim())
            .Build();

        await _repo.AddAsync(lembrete);
        await _repo.SaveChangesAsync();

        return lembrete.ToResponse();
    }
}
