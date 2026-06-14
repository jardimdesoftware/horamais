using System;

namespace Back.Application.DTOs.LembreteEmail;

public record LembreteEmailRequest(
    DateTime Data,
    string? Mensagem
);
