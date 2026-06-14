using Back.Application.DTOs.Aluno;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Aluno
{
    public class UpdateAlunoUseCase
    {
        private readonly IAlunoRepository _alunoRepo;
        private readonly ITurmaRepository _turmaRepo;
        private readonly IIdentityService _identityService;

        public UpdateAlunoUseCase(
            IAlunoRepository alunoRepo,
            ITurmaRepository turmaRepo,
            IIdentityService identityService)
        {
            _alunoRepo = alunoRepo;
            _turmaRepo = turmaRepo;
            _identityService = identityService;
        }

        public async Task<UpdateAlunoResponse> ExecuteAsync(Guid id, UpdateAlunoRequest request)
        {
            // 1. Busca o aluno existente
            var aluno = await _alunoRepo.GetByIdAsync(id);
            if (aluno == null)
                throw new InvalidOperationException("Aluno não encontrado.");

            // 2. Verifica se a turma mudou e valida a nova turma
            if (aluno.TurmaId != request.TurmaId)
            {
                var turma = await _turmaRepo.GetByIdAsync(request.TurmaId);
                if (turma == null)
                    throw new InvalidOperationException("Turma não encontrada.");

                // Atualiza a TurmaId no objeto aluno
                aluno.TurmaId = request.TurmaId;
            }

            // 3. Atualiza os dados no Identity
            //    que lida com a atualização de email e senha (se fornecida).
            var (success, errors) = await _identityService.UpdateUserAsync(
                aluno.IdentityUserId!,
                request.Email,
                request.Senha // Passa a senha (null/vazia se não for para alterar)
            );

            if (!success)
                throw new InvalidOperationException("Erro ao atualizar dados de usuário: " + string.Join("; ", errors));

            // 4. Atualiza os dados locais do aluno
            aluno.Nome = request.Nome;
            aluno.Email = request.Email;
            aluno.Matricula = request.Matricula;
            // O TurmaId já foi atualizado no passo 2, se necessário

            // 5. Persiste as mudanças no banco
            await _alunoRepo.UpdateAsync(aluno);

            // 6. Retorna a resposta
            return new UpdateAlunoResponse(aluno.Id, aluno.Nome, aluno.Email);
        }
    }
}