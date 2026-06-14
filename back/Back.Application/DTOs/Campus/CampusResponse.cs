using System;

namespace Back.Application.DTOs.Campus;

public record CampusResponse(
    Guid Id,
    string Nome,
    string Cidade
);
