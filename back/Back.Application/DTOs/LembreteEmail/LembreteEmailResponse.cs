using System;

namespace Back.Application.DTOs.LembreteEmail;

public record LembreteEmailResponse(
    Guid Id,
    Guid CursoId,
    DateTime Data,
    string? Mensagem,
    bool Enviado,
    DateTime? EnviadoEm
);
