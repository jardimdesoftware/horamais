using Back.Application.DTOs.Atividade;
using Back.Application.Interfaces.Repositories;
using System.Threading.Tasks;
using System;

namespace Back.Application.UseCases.Atividade;

public class UpdateAtividadeUseCase
{
    private readonly IAtividadeRepository _atividadeRepo;

    public UpdateAtividadeUseCase(IAtividadeRepository atividadeRepo)
    {
        _atividadeRepo = atividadeRepo;
    }

    public async Task ExecuteAsync(Guid id, UpdateAtividadeRequest request)
    {
        var atividade = await _atividadeRepo.GetByIdAsync(id);

        if (atividade == null)
            throw new InvalidOperationException("Atividade não encontrada.");

        // Atualiza as propriedades
        var possuiCurricularizacao = request.Tipo == Back.Domain.Entities.Atividade.TipoAtividade.EXTENSAO
            && request.PossuiCurricularizacaoExtensao;

        if (possuiCurricularizacao && (request.HorasCurricularizacaoExtensao is null || request.HorasCurricularizacaoExtensao <= 0))
            throw new ArgumentException("Informe as horas de curricularização de extensão.");

        atividade.Nome = request.Nome;
        atividade.Grupo = request.Grupo;
        atividade.Categoria = request.Categoria;
        atividade.CategoriaKey = request.CategoriaKey;
        atividade.CargaMaximaSemestral = request.CargaMaximaSemestral;
        atividade.CargaMaximaCurso = request.CargaMaximaCurso;
        atividade.Tipo = request.Tipo;
        atividade.PossuiCurricularizacaoExtensao = possuiCurricularizacao;
        atividade.HorasCurricularizacaoExtensao = possuiCurricularizacao ? request.HorasCurricularizacaoExtensao : null;

        await _atividadeRepo.UpdateAsync(atividade);
    }
}