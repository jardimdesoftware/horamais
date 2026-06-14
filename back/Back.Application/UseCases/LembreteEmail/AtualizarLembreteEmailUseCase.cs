using Back.Application.DTOs.LembreteEmail;
using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.UseCases.LembreteEmail;

public class AtualizarLembreteEmailUseCase
{
    private readonly ILembreteEmailRepository _repo;

    public AtualizarLembreteEmailUseCase(ILembreteEmailRepository repo)
    {
        _repo = repo;
    }

    public async Task<LembreteEmailResponse> ExecuteAsync(Guid cursoId, Guid id, LembreteEmailRequest request)
    {
        var lembrete = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Lembrete não encontrado.");

        if (lembrete.CursoId != cursoId)
            throw new UnauthorizedAccessException("Este lembrete não pertence ao seu curso.");

        if (lembrete.Enviado)
            throw new InvalidOperationException("Não é possível editar um lembrete já enviado.");

        var data = DateTime.SpecifyKind(request.Data.Date, DateTimeKind.Utc);
        if (data < DateTime.UtcNow.Date)
            throw new ArgumentException("Não é possível cadastrar uma data de lembrete no passado.");

        lembrete.Data = data;
        lembrete.MensagemPersonalizada = string.IsNullOrWhiteSpace(request.Mensagem) ? null : request.Mensagem.Trim();

        await _repo.UpdateAsync(lembrete);
        await _repo.SaveChangesAsync();

        return lembrete.ToResponse();
    }
}
