using Back.Application.Interfaces.Repositories;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Back.Application.UseCases.LembreteEmail;

/// <summary>
/// Resolve o curso do coordenador autenticado a partir do token.
/// </summary>
public class ResolverCursoCoordenadorUseCase
{
    private readonly ICoordenadorRepository _coordenadorRepo;

    public ResolverCursoCoordenadorUseCase(ICoordenadorRepository coordenadorRepo)
    {
        _coordenadorRepo = coordenadorRepo;
    }

    public async Task<Guid> ExecuteAsync(ClaimsPrincipal user)
    {
        var identityUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Usuário não identificado.");

        var coordenador = await _coordenadorRepo.GetByIdentityUserIdWithCursoAsync(identityUserId)
            ?? throw new UnauthorizedAccessException("Coordenador não encontrado.");

        return coordenador.CursoId;
    }
}
