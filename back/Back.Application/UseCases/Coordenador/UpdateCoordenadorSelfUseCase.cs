using Back.Application.DTOs.Coordenador;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Coordenador;

public class UpdateCoordenadorSelfUseCase
{
    private readonly ICoordenadorRepository _coordenadorRepo;
    private readonly IIdentityService _identity;

    public UpdateCoordenadorSelfUseCase(
        ICoordenadorRepository coordenadorRepo,
        IIdentityService identity)
    {
        _coordenadorRepo = coordenadorRepo;
        _identity = identity;
    }

    public async Task ExecuteAsync(ClaimsPrincipal user, UpdateCoordenadorSelfRequest request)
    {
        var identityUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(identityUserId))
            throw new InvalidOperationException("Usuário não autenticado.");

        // 1. Busca o coordenador (precisa ser rastreado pelo EF, por isso um novo método no repo)
        var coordenador = await _coordenadorRepo.GetByIdentityUserIdAsync(identityUserId);
        if (coordenador == null)
            throw new InvalidOperationException("Coordenador não encontrado.");

        // 2. Atualiza o usuário no Identity
        var (success, errors) = await _identity.UpdateUserAsync(
            coordenador.IdentityUserId!,
            request.Email,
            request.Senha); // Passa a senha (null se não for alterar)

        if (!success)
            throw new InvalidOperationException("Erro ao atualizar dados de usuário: " + string.Join("; ", errors));

        // 3. Atualiza os dados locais do coordenador
        // O coordenador não pode alterar o próprio CursoId
        coordenador.Nome = request.Nome;
        coordenador.Email = request.Email;
        coordenador.NumeroPortaria = request.NumeroPortaria;
        coordenador.DOU = request.DOU;

        // 4. Persiste as mudanças
        await _coordenadorRepo.UpdateAsync(coordenador);
    }
}