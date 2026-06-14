using Back.Application.DTOs.Coordenador;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Coordenador;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Coordenador;

public class CriarCoordenadorUseCase
{
    private readonly IConviteCoordenadorRepository _conviteRepo;
    private readonly ICoordenadorRepository _coordenadorRepo;
    private readonly IIdentityService _identity;

    public CriarCoordenadorUseCase(
        IConviteCoordenadorRepository conviteRepo,
        ICoordenadorRepository coordenadorRepo,
        IIdentityService identity)
    {
        _conviteRepo = conviteRepo;
        _coordenadorRepo = coordenadorRepo;
        _identity = identity;
    }

    public async Task<CoordenadorResponse> ExecuteAsync(CadastroCoordenadorRequest request)
    {
        // 0. Validar domínio do email
        if (!request.Email.EndsWith("@ifpe.edu.br", StringComparison.OrdinalIgnoreCase) &&
            !request.Email.EndsWith(".ifpe.edu.br", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Email institucional inválido.");

        // 1. Validar o convite
        var convite = await _conviteRepo.GetValidByTokenAsync(request.Token);
        if (convite == null)
            throw new InvalidOperationException("Token inválido ou expirado.");

        // 2. Criar usuário no Identity
        var (success, userId, errors) = await _identity.CreateUserAsync(request.Email, request.Senha, "COORDENADOR");
        if (!success)
            throw new InvalidOperationException("Erro ao criar usuário: " + string.Join("; ", errors));

        // 3. Criar coordenador no domínio
        var coordenador = new CoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithNome(request.Nome)
            .WithEmail(request.Email)
            .WithNumeroPortaria(request.NumeroPortaria)
            .WithDOU(request.DOU)
            .WithCursoId(convite.CursoId)
            .WithIdentityUserId(userId)
            .Build();

        await _coordenadorRepo.AddAsync(coordenador);

        // 4. Marcar convite como usado
        await _conviteRepo.MarcarComoUsadoAsync(convite);

        return new CoordenadorResponse(coordenador.Id, coordenador.Nome, coordenador.Email);
    }
}
