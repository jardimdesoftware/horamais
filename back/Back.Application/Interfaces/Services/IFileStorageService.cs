using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Back.Application.Interfaces.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Faz upload de um arquivo e retorna a chave (object key) gerada no storage.
    /// </summary>
    Task<string> UploadAsync(IFormFile file, string objectKey);

    /// <summary>
    /// Baixa um arquivo do storage e retorna o stream e o content type.
    /// </summary>
    Task<(Stream Content, string ContentType)> DownloadAsync(string objectKey);

    /// <summary>
    /// Remove um arquivo do storage. Não lança exceção se o arquivo não existir.
    /// </summary>
    Task DeleteAsync(string objectKey);
}
