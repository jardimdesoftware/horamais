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
        if (file.Length == 0 || file.Length > MaxFileSizeBytes)
            throw new ArgumentException("O arquivo excede o tamanho máximo permitido de 5MB.");

        if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            throw new ArgumentException("Tipo de arquivo inválido. Apenas PDF, JPG, JPEG ou PNG são aceitos.");

        Span<byte> header = stackalloc byte[8];
        using var stream = file.OpenReadStream();
        if (stream.Read(header) < 8)
            throw new ArgumentException("Arquivo inválido.");
        var isPdf = header[..5].SequenceEqual("%PDF-"u8);
        var isPng = header.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        var isJpeg = header[..3].SequenceEqual(new byte[] { 255, 216, 255 });
        var valid = file.ContentType.ToLowerInvariant() switch
        {
            "application/pdf" => isPdf,
            "image/png" => isPng,
            "image/jpeg" or "image/jpg" => isJpeg,
            _ => false
        };
        if (!valid)
            throw new ArgumentException("O conteúdo do arquivo não corresponde ao tipo informado.");
    }

    public static async Task<byte[]> ToByteArrayAsync(this IFormFile file)
    {
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ms.ToArray();
    }
}
