using Back.Application.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Back.Domain.Entities.AlunoAtividade;

namespace Back.Application.UseCases.Turma;

public class DeleteTurmaUseCase
{
    private readonly ITurmaRepository _turmaRepo;
    private readonly IAlunoRepository _alunoRepo;
    private readonly IAlunoAtividadeRepository _alunoAtividadeRepo;
    private readonly ICertificadoRepository _certificadoRepo;

    public DeleteTurmaUseCase(
        ITurmaRepository turmaRepo,
        IAlunoRepository alunoRepo,
        IAlunoAtividadeRepository alunoAtividadeRepo,
        ICertificadoRepository certificadoRepo)
    {
        _turmaRepo = turmaRepo;
        _alunoRepo = alunoRepo;
        _alunoAtividadeRepo = alunoAtividadeRepo;
        _certificadoRepo = certificadoRepo;
    }

    public async Task ExecuteAsync(string identifier)
    {
        // 1. Busca a turma (rastreada)
        var turma = await _turmaRepo.GetByIdentifierTrackedAsync(identifier);
        if (turma == null)
            throw new KeyNotFoundException("Turma não encontrada.");

        // 2. Busca todos os alunos da turma (rastreados)
        var alunos = await _alunoRepo.GetByTurmaIdTrackedAsync(turma.Id);
        if (alunos.Any())
        {
            // 3. Busca AlunoAtividades
            var alunoAtividades = new List<AlunoAtividade>();
            foreach (var aluno in alunos)
            {
                alunoAtividades.AddRange(await _alunoAtividadeRepo.GetByAlunoIdTrackedAsync(aluno.Id));
            }

            if (alunoAtividades.Any())
            {
                // 4. Busca Certificados
                var certificados = new List<Back.Domain.Entities.Certificado.Certificado>();
                foreach (var aa in alunoAtividades)
                {
                    certificados.AddRange(await _certificadoRepo.GetByAlunoAtividadeIdTrackedAsync(aa.Id));
                }

                // 5. DELETAR Certificados (Agora a lista 'certificados' tem o tipo correto)
                await _certificadoRepo.RemoveRangeAsync(certificados);
            }

            // 6. DELETAR AlunoAtividades
            await _alunoAtividadeRepo.RemoveRangeAsync(alunoAtividades);

            // 7. DELETAR Alunos (e suas contas do Identity)
            foreach (var aluno in alunos)
            {
                await _alunoRepo.DeleteComIdentityAsync(aluno.Id);
            }
        }

        // 8. DELETAR a Turma
        await _turmaRepo.DeleteAsync(turma);
    }
}