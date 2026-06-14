using Back.Application.Interfaces.Repositories;
using Back.Application.Interfaces.Services;
using Back.Domain.Entities.Certificado;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Certificado
{
    public class DeleteCertificadoUseCase
    {
        private readonly ICertificadoRepository _certRepo;
        private readonly IFileStorageService _storage;

        public DeleteCertificadoUseCase(ICertificadoRepository certRepo, IFileStorageService storage)
        {
            _certRepo = certRepo;
            _storage = storage;
        }

        public async Task ExecuteAsync(Guid id)
        {
            var certificado = await _certRepo.GetByIdAsync(id);

            if (certificado == null)
                throw new KeyNotFoundException("Certificado não encontrado.");

            if (certificado.Status == StatusCertificado.APROVADO)
                throw new InvalidOperationException("Não é possível excluir um certificado que já foi APROVADO.");

            var storageKey = certificado.AnexoStorageKey;

            await _certRepo.DeleteAsync(certificado);

            if (!string.IsNullOrEmpty(storageKey))
                await _storage.DeleteAsync(storageKey);
        }
    }
}
