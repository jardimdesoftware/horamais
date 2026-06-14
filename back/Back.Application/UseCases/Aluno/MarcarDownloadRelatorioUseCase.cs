using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Atividade;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Aluno;

public class MarcarDownloadRelatorioUseCase
{
    private readonly IAlunoRepository _alunoRepo;
    private readonly ICoordenadorRepository _coordenadorRepo;

    public MarcarDownloadRelatorioUseCase(IAlunoRepository alunoRepo, ICoordenadorRepository coordenadorRepo)
    {
        _alunoRepo = alunoRepo;
        _coordenadorRepo = coordenadorRepo;
    }

    public async Task ExecuteAsync(Guid alunoId, TipoAtividade tipo, ClaimsPrincipal user)
    {
        // 1. Identificar o Coordenador e seu Curso a partir do Token
        var identityUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Usuário não identificado no token.");

        var coordenador = await _coordenadorRepo.GetByIdentityUserIdWithCursoAsync(identityUserId)
            ?? throw new UnauthorizedAccessException("Coordenador não encontrado.");

        if (coordenador.Curso == null)
        {
            throw new InvalidOperationException("O coordenador não está associado a nenhum curso.");
        }

        // 2. Buscar o aluno que será modificado
        // Usamos GetByIdWithAtividadesAsync para garantir que a propriedade Turma seja carregada
        var aluno = await _alunoRepo.GetByIdWithAtividadesAsync(alunoId)
            ?? throw new KeyNotFoundException("Aluno não encontrado.");

        // 3. VERIFICAÇÃO DE SEGURANÇA CRÍTICA
        // Garante que o coordenador só pode modificar alunos do seu próprio curso.
        if (aluno.Turma?.CursoId != coordenador.Curso.Id)
        {
            throw new UnauthorizedAccessException("O coordenador não tem permissão para alterar dados de alunos de outro curso.");
        }

        // 4. Alterar o status apropriado com base no tipo
        switch (tipo)
        {
            case TipoAtividade.COMPLEMENTAR:
                aluno.JaBaixadoHorasComplementares = true;
                break;
            case TipoAtividade.EXTENSAO:
                aluno.JaBaixadoHorasExtensao = true;
                break;
            default:
                // Lança uma exceção se um tipo inválido for passado
                throw new ArgumentOutOfRangeException(nameof(tipo), "Tipo de atividade inválido para esta operação.");
        }

        // 5. Salvar a alteração no banco de dados
        await _alunoRepo.UpdateAsync(aluno);
    }
}