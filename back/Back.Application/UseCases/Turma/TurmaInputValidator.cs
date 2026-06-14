using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Back.Application.UseCases.Turma;

/// <summary>
/// Validação do período letivo e normalização do turno usados na criação e
/// atualização de turmas. O turno é normalizado para um conjunto fechado de
/// valores canônicos (minúsculos, sem acento) — os mesmos enviados pelo front.
/// </summary>
internal static class TurmaInputValidator
{
    private static readonly Regex PeriodoRegex = new(@"^\d{4}\.[12]$", RegexOptions.Compiled);

    private static readonly HashSet<string> TurnosValidos = new() { "manha", "tarde", "noite" };

    public static string ValidarPeriodo(string? periodo)
    {
        var valor = periodo?.Trim() ?? string.Empty;
        if (!PeriodoRegex.IsMatch(valor))
            throw new ArgumentException("Período inválido. Use o formato AAAA.1 ou AAAA.2 (ex.: 2026.1).");
        return valor;
    }

    public static string NormalizarTurno(string? turno)
    {
        var canonico = RemoverAcentos(turno?.Trim() ?? string.Empty).ToLowerInvariant();
        if (!TurnosValidos.Contains(canonico))
            throw new ArgumentException("Turno inválido. Valores aceitos: manhã, tarde ou noite.");
        return canonico;
    }

    private static string RemoverAcentos(string texto)
    {
        var decomposto = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposto.Length);
        foreach (var c in decomposto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
