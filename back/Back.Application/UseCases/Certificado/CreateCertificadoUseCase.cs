using Back.Application.DTOs.Certificado;
using Back.Application.Extensions;
using Back.Application.Interfaces.Repositories;
using Back.Application.Interfaces.Services;
using Back.Domain.Entities.Certificado;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Certificado;

public class CreateCertificadoUseCase
{
    private readonly IAlunoAtividadeRepository _alunoAtividadeRepo;
    private readonly ICertificadoRepository _certificadoRepo;
    private readonly ILimiteHorasAlunoRepository _limiteRepo;
    private readonly IAtividadeRepository _atividadeRepo;
    private readonly IFileStorageService _storage;
    private readonly ValidarLimiteCertificadoUseCase _validarLimite;

    public CreateCertificadoUseCase(
        IAlunoAtividadeRepository alunoAtividadeRepo,
        ICertificadoRepository certificadoRepo,
        ILimiteHorasAlunoRepository limiteRepo,
        IAtividadeRepository atividadeRepo,
        IFileStorageService storage,
        ValidarLimiteCertificadoUseCase validarLimite)
    {
        _alunoAtividadeRepo = alunoAtividadeRepo;
        _certificadoRepo = certificadoRepo;
        _limiteRepo = limiteRepo;
        _atividadeRepo = atividadeRepo;
        _storage = storage;
        _validarLimite = validarLimite;
    }

    public async Task<Guid> ExecuteAsync(CreateCertificadoRequest request)
    {
        if (request.Anexo == null || request.Anexo.Length == 0)
            throw new InvalidOperationException("O anexo é obrigatório e deve conter conteúdo válido.");

        request.Anexo.ValidateAnexo();

        var alunoAtividade = await _alunoAtividadeRepo
            .GetByAlunoEAtividadeAsync(request.AlunoId, request.AtividadeId);

        // Atividades são globais: se o vínculo não existe, cria automaticamente
        if (alunoAtividade == null)
        {
            var atividade = await _atividadeRepo.GetByIdAsync(request.AtividadeId);
            if (atividade == null)
                throw new InvalidOperationException("Atividade não encontrada.");

            alunoAtividade = new Back.Domain.Entities.AlunoAtividade.AlunoAtividade
            {
                Id = Guid.NewGuid(),
                AlunoId = request.AlunoId,
                AtividadeId = request.AtividadeId,
                HorasConcluidas = 0,
                Atividade = atividade
            };
            await _alunoAtividadeRepo.AddRangeAsync(new[] { alunoAtividade });
        }

        await _validarLimite.ExecuteAsync(
            alunoAtividade.Id,
            request.CargaHoraria,
            request.PeriodoLetivo,
            alunoAtividade.Atividade!.CargaMaximaSemestral,
            alunoAtividade.Atividade!.CargaMaximaCurso);

        var certificadoId = Guid.NewGuid();
        var extension = request.Anexo.ContentType.ToLowerInvariant() switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png"                 => ".png",
            _                           => ".pdf"
        };
        var storageKey = $"certificados/{certificadoId}{extension}";
        await _storage.UploadAsync(request.Anexo, storageKey);

        var certificado = new CertificadoBuilder()
            .WithId(certificadoId)
            .WithTituloAtividade(request.TituloAtividade)
            .WithInstituicao(request.Instituicao)
            .WithLocal(request.Local)
            .WithCategoria(request.Categoria)
            .WithGrupo(request.Grupo)
            .WithPeriodoLetivo(request.PeriodoLetivo)
            .WithCargaHoraria(request.CargaHoraria)
            .WithDataInicio(request.DataInicio)
            .WithDataFim(request.DataFim)
            .WithTotalPeriodos(request.TotalPeriodos)
            .WithDescricao(request.Descricao)
            .WithAnexoStorageKey(storageKey)
            .WithTipo(request.Tipo)
            .WithAlunoAtividadeId(alunoAtividade.Id)
            .Build();

        await _certificadoRepo.AddAsync(certificado);
        return certificado.Id;
    }
}
