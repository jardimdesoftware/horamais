using Back.Application.DTOs.Atividade;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.AlunoAtividade;
using System.Threading.Tasks;
using System;
using EntidadeAtividade = Back.Domain.Entities.Atividade.Atividade;
using System.Linq;

namespace Back.Application.UseCases.Atividade;

public class CreateAtividadeUseCase
{
    private readonly IAtividadeRepository _atividadeRepo;
    private readonly IAlunoRepository _alunoRepo;
    private readonly IAlunoAtividadeRepository _alunoAtividadeRepo;

    public CreateAtividadeUseCase(
        IAtividadeRepository atividadeRepo,
        IAlunoRepository alunoRepo,
        IAlunoAtividadeRepository alunoAtividadeRepo)
    {
        _atividadeRepo = atividadeRepo;
        _alunoRepo = alunoRepo;
        _alunoAtividadeRepo = alunoAtividadeRepo;
    }

    public async Task<Guid> ExecuteAsync(CreateAtividadeRequest request)
    {
        var possuiCurricularizacao = request.Tipo == Back.Domain.Entities.Atividade.TipoAtividade.EXTENSAO
            && request.PossuiCurricularizacaoExtensao;

        if (possuiCurricularizacao && (request.HorasCurricularizacaoExtensao is null || request.HorasCurricularizacaoExtensao <= 0))
            throw new ArgumentException("Informe as horas de curricularização de extensão.");

        var atividade = new EntidadeAtividade
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Grupo = request.Grupo,
            Categoria = request.Categoria,
            CategoriaKey = request.CategoriaKey,
            CargaMaximaSemestral = request.CargaMaximaSemestral,
            CargaMaximaCurso = request.CargaMaximaCurso,
            Tipo = request.Tipo,
            PossuiCurricularizacaoExtensao = possuiCurricularizacao,
            HorasCurricularizacaoExtensao = possuiCurricularizacao ? request.HorasCurricularizacaoExtensao : null
        };

        await _atividadeRepo.AddAsync(atividade);

        // Atividades são globais: vincula a TODOS os alunos do sistema
        var alunos = await _alunoRepo.GetAllAsync();

        var alunoAtividades = alunos.Select(aluno => new AlunoAtividade
        {
            Id = Guid.NewGuid(),
            AlunoId = aluno.Id,
            AtividadeId = atividade.Id,
            HorasConcluidas = 0
        });

        await _alunoAtividadeRepo.AddRangeAsync(alunoAtividades);

        return atividade.Id;
    }
}
