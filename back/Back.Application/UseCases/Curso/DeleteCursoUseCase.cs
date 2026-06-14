using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Back.Domain.Entities.AlunoAtividade;
using Back.Domain.Entities.Certificado;


namespace Back.Application.UseCases.Curso
{
    public class DeleteCursoUseCase
    {
        private readonly ICursoRepository _cursoRepo;
        private readonly ILimiteHorasAlunoRepository _limiteRepo;
        private readonly ICoordenadorRepository _coordenadorRepo;
        private readonly ITurmaRepository _turmaRepo;
        private readonly IAlunoRepository _alunoRepo;
        private readonly IAlunoAtividadeRepository _alunoAtividadeRepo;
        private readonly ICertificadoRepository _certificadoRepo;

        public DeleteCursoUseCase(
            ICursoRepository cursoRepo,
            ILimiteHorasAlunoRepository limiteRepo,
            ICoordenadorRepository coordenadorRepo,
            ITurmaRepository turmaRepo,
            IAlunoRepository alunoRepo,
            IAlunoAtividadeRepository alunoAtividadeRepo,
            ICertificadoRepository certificadoRepo)
        {
            _cursoRepo = cursoRepo;
            _limiteRepo = limiteRepo;
            _coordenadorRepo = coordenadorRepo;
            _turmaRepo = turmaRepo;
            _alunoRepo = alunoRepo;
            _alunoAtividadeRepo = alunoAtividadeRepo;
            _certificadoRepo = certificadoRepo;
        }

        public async Task ExecuteAsync(Guid id)
        {
            var curso = await _cursoRepo.GetByIdAsync(id);
            if (curso == null)
                throw new KeyNotFoundException("Curso não encontrado.");

            // 2. Buscar Turmas (Rastreado)
            var turmas = await _turmaRepo.GetByCursoIdTrackedAsync(id);
            if (turmas.Any())
            {
                // 3. Buscar Alunos (Rastreado)
                // --- CORREÇÃO AQUI ---
                var alunos = new List<Back.Domain.Entities.Aluno.Aluno>();
                foreach (var turma in turmas)
                {
                    alunos.AddRange(await _alunoRepo.GetByTurmaIdTrackedAsync(turma.Id));
                }

                if (alunos.Any())
                {
                    // 4. Buscar AlunoAtividades (Rastreado)
                    var alunoAtividades = new List<AlunoAtividade>();
                    foreach (var aluno in alunos)
                    {
                        alunoAtividades.AddRange(await _alunoAtividadeRepo.GetByAlunoIdTrackedAsync(aluno.Id));
                    }

                    if (alunoAtividades.Any())
                    {
                        // 5. Buscar Certificados (Rastreado)
                        // --- CORREÇÃO AQUI ---
                        var certificados = new List<Back.Domain.Entities.Certificado.Certificado>();
                        foreach (var aa in alunoAtividades)
                        {
                            certificados.AddRange(await _certificadoRepo.GetByAlunoAtividadeIdTrackedAsync(aa.Id));
                        }

                        // 6. DELETAR Certificados
                        await _certificadoRepo.RemoveRangeAsync(certificados);
                    }

                    // 7. DELETAR AlunoAtividades
                    await _alunoAtividadeRepo.RemoveRangeAsync(alunoAtividades);
                }

                // 8. DELETAR Alunos (e suas contas do Identity)
                foreach (var aluno in alunos)
                {
                    await _alunoRepo.DeleteComIdentityAsync(aluno.Id);
                }
            }

            // 9. DELETAR Turmas
            await _turmaRepo.RemoveRangeAsync(turmas);

            // Atividades são globais: NÃO deletar ao remover curso

            // 10. DELETAR Coordenador (e sua conta do Identity)
            var coordenador = await _coordenadorRepo.GetByCursoIdAsync(id);
            if (coordenador != null)
            {
                await _coordenadorRepo.DeleteComIdentityAsync(coordenador.Id);
            }

            // 12. DELETAR LimiteHorasAluno
            var limite = await _limiteRepo.GetByCursoIdToUpdateAsync(id); // Pega rastreado
            if (limite != null)
            {
                await _limiteRepo.DeleteAsync(limite);
            }

            // 13. DELETAR o Curso
            var cursoParaDeletar = await _cursoRepo.GetByIdToUpdateAsync(id); // Pega rastreado
            if (cursoParaDeletar != null)
            {
                await _cursoRepo.DeleteAsync(cursoParaDeletar);
            }
        }
    }
}