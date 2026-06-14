using Back.Application.Interfaces.Repositories;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Back.Application.UseCases.Atividade
{
    public class DeleteAtividadeUseCase
    {
        private readonly IAtividadeRepository _atividadeRepo;
        private readonly IAlunoAtividadeRepository _alunoAtividadeRepo;

        public DeleteAtividadeUseCase(
            IAtividadeRepository atividadeRepo,
            IAlunoAtividadeRepository alunoAtividadeRepo)
        {
            _atividadeRepo = atividadeRepo;
            _alunoAtividadeRepo = alunoAtividadeRepo;
        }

        public async Task ExecuteAsync(Guid id)
        {
            var atividade = await _atividadeRepo.GetByIdAsync(id);
            if (atividade == null)
                throw new InvalidOperationException("Atividade não encontrada.");

            // 1. Encontra todos os vínculos AlunoAtividade
            // Assumindo que seu repositório tenha este método:
            var vinculos = await _alunoAtividadeRepo.GetByAtividadeIdAsync(id);

            // 2. Remove os vínculos
            if (vinculos.Any())
            {
                // Assumindo que seu repositório tenha este método:
                await _alunoAtividadeRepo.RemoveRangeAsync(vinculos);
            }

            // 3. Remove a atividade principal
            await _atividadeRepo.DeleteAsync(atividade);
        }
    }
}