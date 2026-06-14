using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Back.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, mensagem) = MapException(exception);

        var sanitizedMethod = context.Request.Method.Replace("\r", string.Empty).Replace("\n", string.Empty);
        var sanitizedPath = context.Request.Path.ToString().Replace("\r", string.Empty).Replace("\n", string.Empty);

        if (statusCode >= 500)
            _logger.LogError(exception, "Erro não tratado na requisição {Method} {Path}", sanitizedMethod, sanitizedPath);
        else
            _logger.LogWarning("Requisição rejeitada ({Status}): {Mensagem}", statusCode, mensagem);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var body = JsonSerializer.Serialize(new { mensagem }, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(body);
    }

    private static (int statusCode, string mensagem) MapException(Exception exception) => exception switch
    {
        ArgumentException ex
            => (StatusCodes.Status400BadRequest, ex.Message),

        KeyNotFoundException ex
            => (StatusCodes.Status404NotFound, ex.Message),

        UnauthorizedAccessException ex
            => (StatusCodes.Status403Forbidden, ex.Message),

        InvalidOperationException ex when IsConfigError(ex)
            => (StatusCodes.Status500InternalServerError, "Erro de configuração do servidor. Contate o administrador."),

        InvalidOperationException ex
            => (StatusCodes.Status422UnprocessableEntity, ex.Message),

        // MailKit.Security.AuthenticationException e SmtpCommandException chegam como Exception
        Exception ex when IsSmtpAuthError(ex)
            => (StatusCodes.Status502BadGateway, "Falha na autenticação com o servidor de e-mail. Verifique as credenciais SMTP."),

        Exception ex when IsSmtpError(ex)
            => (StatusCodes.Status502BadGateway, "Não foi possível conectar ao servidor de e-mail. Tente novamente mais tarde."),

        _ => (StatusCodes.Status500InternalServerError, "Ocorreu um erro interno. Tente novamente mais tarde.")
    };

    private static bool IsConfigError(InvalidOperationException ex) =>
        ex.Message.Contains("não configurad", StringComparison.OrdinalIgnoreCase) ||
        ex.Message.Contains("Configurações de e-mail", StringComparison.OrdinalIgnoreCase);

    private static bool IsSmtpAuthError(Exception ex)
    {
        var typeName = ex.GetType().FullName ?? string.Empty;
        return typeName.Contains("AuthenticationException") ||
               (ex.Message.Contains("Username and Password not accepted", StringComparison.OrdinalIgnoreCase)) ||
               (ex.Message.Contains("535", StringComparison.Ordinal));
    }

    private static bool IsSmtpError(Exception ex)
    {
        var typeName = ex.GetType().FullName ?? string.Empty;
        return typeName.StartsWith("MailKit.") || typeName.StartsWith("MimeKit.");
    }
}
