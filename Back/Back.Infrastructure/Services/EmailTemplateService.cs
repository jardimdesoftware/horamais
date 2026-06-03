using Back.Application.Interfaces;
using System;
using System.IO;
using System.Net;
using System.Reflection;

namespace Back.Infrastructure.Services;

public class EmailTemplateService : IEmailTemplateService
{
    private static readonly Assembly _assembly = typeof(EmailTemplateService).Assembly;

    public string RenderConviteCoordenador(string link)
    {
        var template = LoadTemplate("convite-coordenador.html");
        return template
            .Replace("{{LINK}}", WebUtility.HtmlEncode(link))
            .Replace("{{ANO}}", DateTime.UtcNow.Year.ToString());
    }

    public string RenderResetSenha(string nome, string codigo)
    {
        // Garante 6 dígitos mesmo que o código tenha zeros à esquerda
        var digits = codigo.PadLeft(6, '0');

        var template = LoadTemplate("reset-senha.html");
        return template
            .Replace("{{NOME}}", WebUtility.HtmlEncode(nome))
            .Replace("{{D1}}", digits[0].ToString())
            .Replace("{{D2}}", digits[1].ToString())
            .Replace("{{D3}}", digits[2].ToString())
            .Replace("{{D4}}", digits[3].ToString())
            .Replace("{{D5}}", digits[4].ToString())
            .Replace("{{D6}}", digits[5].ToString())
            .Replace("{{ANO}}", DateTime.UtcNow.Year.ToString());
    }

    public string RenderLembretePendencia(string nome, string mensagem)
    {
        var template = LoadTemplate("lembrete-pendencia.html");
        return template
            .Replace("{{NOME}}", WebUtility.HtmlEncode(nome))
            .Replace("{{MENSAGEM}}", WebUtility.HtmlEncode(mensagem))
            .Replace("{{ANO}}", DateTime.UtcNow.Year.ToString());
    }

    private static string LoadTemplate(string fileName)
    {
        var resourceName = $"Back.Infrastructure.Templates.{fileName}";
        using var stream = _assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Template de e-mail não encontrado: '{resourceName}'. Verifique se o arquivo está marcado como EmbeddedResource.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
