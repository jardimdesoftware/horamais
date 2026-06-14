using Back.Application.DTOs.Coordenador;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Coordenador
{
    public class UpdateCoordenadorAdminUseCase
    {
        private readonly ICoordenadorRepository _coordenadorRepo;
        private readonly IIdentityService _identity;

        public UpdateCoordenadorAdminUseCase(
            ICoordenadorRepository coordenadorRepo,
            IIdentityService identity)
        {
            _coordenadorRepo = coordenadorRepo;
            _identity = identity;
        }

        public async Task ExecuteAsync(Guid id, UpdateCoordenadorAdminRequest request)
        {
            // 1. Busca o coordenador pelo ID
            var coordenador = await _coordenadorRepo.GetByIdAsync(id);
            if (coordenador == null)
                throw new InvalidOperationException("Coordenador não encontrado.");

            // 2. Atualiza o usuário no Identity
            var (success, errors) = await _identity.UpdateUserAsync(
                coordenador.IdentityUserId!,
                request.Email,
                request.Senha);

            if (!success)
                throw new InvalidOperationException("Erro ao atualizar dados de usuário: " + string.Join("; ", errors));

            // 3. Atualiza os dados locais (Admin pode alterar o CursoId)
            coordenador.Nome = request.Nome;
            coordenador.Email = request.Email;
            coordenador.NumeroPortaria = request.NumeroPortaria;
            coordenador.DOU = request.DOU;
            coordenador.CursoId = request.CursoId;

            // 4. Persiste as mudanças
            await _coordenadorRepo.UpdateAsync(coordenador);
        }
    }
}