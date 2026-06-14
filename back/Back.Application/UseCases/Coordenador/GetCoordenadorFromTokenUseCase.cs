using Back.Application.DTOs.Coordenador;
using Back.Application.Interfaces.Repositories;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Coordenador;

public class GetCoordenadorFromTokenUseCase
{
    private readonly ICoordenadorRepository _repo;

    public GetCoordenadorFromTokenUseCase(ICoordenadorRepository repo)
    {
        _repo = repo;
    }

    public async Task<CoordenadorInfoResponse> ExecuteAsync(ClaimsPrincipal user)
    {
        var identityUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Usuário não identificado.");

        var coordenador = await _repo.GetByIdentityUserIdWithCursoAsync(identityUserId)
            ?? throw new UnauthorizedAccessException("Coordenador não encontrado.");

        return new CoordenadorInfoResponse(
            Nome: coordenador.Nome,
            Curso: coordenador.Curso?.Nome ?? "Curso não encontrado",
            NumeroPortaria: coordenador.NumeroPortaria,
            DOU: coordenador.DOU
        );
    }
}
