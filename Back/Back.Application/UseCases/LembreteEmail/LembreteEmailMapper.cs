using Back.Application.DTOs.LembreteEmail;
using Back.Domain.Entities.LembreteEmail;

namespace Back.Application.UseCases.LembreteEmail;

internal static class LembreteEmailMapper
{
    public static LembreteEmailResponse ToResponse(this Domain.Entities.LembreteEmail.LembreteEmail lembrete) =>
        new(
            lembrete.Id,
            lembrete.CursoId,
            lembrete.Data,
            lembrete.MensagemPersonalizada,
            lembrete.Enviado,
            lembrete.EnviadoEm
        );
}
