using System;

namespace Back.Application.DTOs.Coordenador;

public record ConviteCoordenadorRequest(string Email, Guid CursoId);
