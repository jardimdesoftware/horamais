using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Application.Extensions;

public static class FileExtensions
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    private static readonly string[] AllowedMimeTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/jpg",
        "image/png"
    ];

    public static void ValidateAnexo(this IFormFile file)
    {
        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException("O arquivo excede o tamanho máximo permitido de 5MB.");

        if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new ArgumentException("Tipo de arquivo inválido. Apenas PDF, JPG, JPEG ou PNG são aceitos.");
    }

    public static async Task<byte[]> ToByteArrayAsync(this IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ms.ToArray();
    }
}
