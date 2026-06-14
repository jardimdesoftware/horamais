using Back.Application.Interfaces.Repositories;
using Back.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Certificado;

public class GetCertificadoAnexoUseCase
{
    private readonly ICertificadoRepository _repo;
    private readonly IFileStorageService _storage;

    public GetCertificadoAnexoUseCase(ICertificadoRepository repo, IFileStorageService storage)
    {
        _repo = repo;
        _storage = storage;
    }

    public async Task<(Stream Content, string NomeArquivo, string ContentType)> ExecuteAsync(Guid certificadoId)
    {
        var storageKey = await _repo.GetStorageKeyByIdAsync(certificadoId);

        if (string.IsNullOrEmpty(storageKey))
            throw new KeyNotFoundException("Anexo não encontrado para este certificado.");

        var (content, contentType) = await _storage.DownloadAsync(storageKey);

        var extension = contentType switch
        {
            "image/jpeg" or "image/jpg" => ".jpg",
            "image/png"                 => ".png",
            _                           => ".pdf"
        };
        var nomeArquivo = $"certificado_{certificadoId}{extension}";

        return (content, nomeArquivo, contentType);
    }
}
