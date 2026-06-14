using Back.Application.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Coordenador
{
    public class DeleteCoordenadorUseCase
    {
        private readonly ICoordenadorRepository _coordenadorRepo;

        public DeleteCoordenadorUseCase(ICoordenadorRepository coordenadorRepo)
        {
            _coordenadorRepo = coordenadorRepo;
        }

        public async Task ExecuteAsync(Guid id)
        {
            // O repositório cuidará de remover o Coordenador e o IdentityUser
            await _coordenadorRepo.DeleteComIdentityAsync(id);
        }
    }
}