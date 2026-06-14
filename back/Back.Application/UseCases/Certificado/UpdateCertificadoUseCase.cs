using Back.Application.DTOs.Certificado;
using Back.Application.Extensions;
using Back.Application.Interfaces.Repositories;
using Back.Application.Interfaces.Services;
using Back.Domain.Entities.Certificado;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Certificado
{
    /// <summary>
    /// Caso de uso para atualizar um certificado.
    /// </summary>
    public class UpdateCertificadoUseCase
    {
        private readonly ICertificadoRepository _certificadoRepo;
        private readonly IAlunoRepository _alunoRepo;
        private readonly IFileStorageService _storage;
        private readonly ValidarLimiteCertificadoUseCase _validarLimite;

        public UpdateCertificadoUseCase(
            ICertificadoRepository certificadoRepo,
            IAlunoRepository alunoRepo,
            IFileStorageService storage,
            ValidarLimiteCertificadoUseCase validarLimite)
        {
            _certificadoRepo = certificadoRepo;
            _alunoRepo = alunoRepo;
            _storage = storage;
            _validarLimite = validarLimite;
        }

        /// <param name="id">O ID do certificado a ser atualizado.</param>
        /// <param name="request">Os novos dados do certificado.</param>
        /// <param name="identityUserId">ID do usuário autenticado (Identity).</param>
        /// <exception cref="KeyNotFoundException">Lançado se o certificado não for encontrado.</exception>
        /// <exception cref="UnauthorizedAccessException">Lançado se o aluno não for dono do certificado.</exception>
        /// <exception cref="InvalidOperationException">Lançado se o certificado já estiver APROVADO.</exception>
        public async Task ExecuteAsync(Guid id, UpdateCertificadoRequest request, string identityUserId)
        {
            // 1. Busca o certificado
            var certificado = await _certificadoRepo.GetByIdAsync(id);

            if (certificado == null)
                throw new KeyNotFoundException("Certificado não encontrado.");

            // 2. Verifica se o aluno autenticado é dono do certificado
            var aluno = await _alunoRepo.GetByIdentityUserIdAsync(identityUserId);
            if (aluno == null || certificado.AlunoAtividade!.AlunoId != aluno.Id)
                throw new UnauthorizedAccessException("Você não tem permissão para alterar este certificado.");

            // 3. REGRA DE NEGÓCIO: não permite alterar certificados APROVADOS
            if (certificado.Status == StatusCertificado.APROVADO)
                throw new InvalidOperationException("Não é possível alterar um certificado que já foi APROVADO.");

            // 4. Valida limites de carga horária excluindo o próprio certificado do somatório
            await _validarLimite.ExecuteAsync(
                certificado.AlunoAtividadeId,
                request.CargaHoraria,
                request.PeriodoLetivo,
                certificado.AlunoAtividade!.Atividade!.CargaMaximaSemestral,
                certificado.AlunoAtividade!.Atividade!.CargaMaximaCurso,
                ignorarCertificadoId: id);

            // 5. Atualiza os campos do certificado
            certificado.TituloAtividade = request.TituloAtividade;
            certificado.Instituicao = request.Instituicao;
            certificado.Local = request.Local;
            certificado.Categoria = request.Categoria;
            certificado.Grupo = request.Grupo;
            certificado.PeriodoLetivo = request.PeriodoLetivo;
            certificado.CargaHoraria = request.CargaHoraria;
            certificado.DataInicio = request.DataInicio;
            certificado.DataFim = request.DataFim;
            certificado.TotalPeriodos = request.TotalPeriodos;
            certificado.Descricao = request.Descricao;
            certificado.Tipo = (TipoCertificado)request.Tipo;

            // 6. Se estava REPROVADO, volta a PENDENTE para reavaliação
            if (certificado.Status == StatusCertificado.REPROVADO)
                certificado.Status = StatusCertificado.PENDENTE;

            // 7. Substitui o anexo no storage se um novo foi enviado
            if (request.Anexo != null && request.Anexo.Length > 0)
            {
                request.Anexo.ValidateAnexo();

                var oldKey = certificado.AnexoStorageKey;

                var extension = request.Anexo.ContentType.ToLowerInvariant() switch
                {
                    "image/jpeg" or "image/jpg" => ".jpg",
                    "image/png"                 => ".png",
                    _                           => ".pdf"
                };
                var newKey = $"certificados/{id}{extension}";

                await _storage.UploadAsync(request.Anexo, newKey);
                certificado.AnexoStorageKey = newKey;

                if (!string.IsNullOrEmpty(oldKey) && oldKey != newKey)
                    await _storage.DeleteAsync(oldKey);
            }

            // 8. Salva as mudanças
            await _certificadoRepo.UpdateAsync(certificado);
        }
    }
}
