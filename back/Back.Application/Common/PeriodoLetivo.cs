using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Back.Application.Common;

/// <summary>
/// Utilitários para o período letivo no formato "AAAA.S" (S = 1 para jan–jun,
/// 2 para jul–dez). Centraliza a validação de formato, a comparação cronológica
/// e a regra de retroatividade: o período mais antigo aceito é <see cref="Minimo"/>
/// e nenhum registro pode ser anterior ao período de ingresso do aluno.
/// </summary>
public static class PeriodoLetivo
{
    /// <summary>Período mais antigo aceito em todo o sistema.</summary>
    public const string Minimo = "2019.2";

    private static readonly Regex Formato = new(@"^\d{4}\.[12]$", RegexOptions.Compiled);

    /// <summary>
    /// Tenta interpretar o período e devolver um valor ordinal comparável
    /// (maior = mais recente). Retorna false se o formato for inválido.
    /// </summary>
    public static bool TryGetOrdinal(string? periodo, out int ordinal)
    {
        ordinal = 0;
        var valor = periodo?.Trim() ?? string.Empty;
        if (!Formato.IsMatch(valor))
            return false;

        var partes = valor.Split('.');
        var ano = int.Parse(partes[0]);
        var semestre = int.Parse(partes[1]);
        ordinal = (ano * 2) + (semestre - 1);
        return true;
    }

    private static int Ordinal(string periodo)
    {
        TryGetOrdinal(periodo, out var ordinal);
        return ordinal;
    }

    /// <summary>Período letivo corrente, derivado da data atual (UTC).</summary>
    public static string Atual()
    {
        var hoje = DateTime.UtcNow;
        var semestre = hoje.Month <= 6 ? 1 : 2;
        return $"{hoje.Year}.{semestre}";
    }

    /// <summary>
    /// Lista, em ordem crescente, os períodos válidos para registro de um aluno:
    /// de max(<see cref="Minimo"/>, período de ingresso) até o período corrente.
    /// </summary>
    public static IReadOnlyList<string> ListarValidosParaAluno(string? periodoIngresso)
    {
        var inicio = Ordinal(Minimo);
        if (TryGetOrdinal(periodoIngresso, out var ordIngresso) && ordIngresso > inicio)
            inicio = ordIngresso;

        var fim = Ordinal(Atual());

        var periodos = new List<string>();
        for (var ord = inicio; ord <= fim; ord++)
        {
            var ano = ord / 2;
            var semestre = (ord % 2) + 1;
            periodos.Add($"{ano}.{semestre}");
        }
        return periodos;
    }

    /// <summary>
    /// Valida um período enviado pelo aluno ao registrar/editar um certificado.
    /// </summary>
    /// <param name="periodo">Período informado no formulário.</param>
    /// <param name="periodoIngresso">Período de ingresso do aluno (da turma); pode ser nulo.</param>
    /// <exception cref="ArgumentException">
    /// Quando o formato é inválido, o período é anterior a <see cref="Minimo"/>
    /// ou anterior ao período de ingresso do aluno.
    /// </exception>
    public static void ValidarRegistro(string? periodo, string? periodoIngresso)
    {
        if (!TryGetOrdinal(periodo, out var ordinal))
            throw new ArgumentException("Período letivo inválido. Use o formato AAAA.1 ou AAAA.2 (ex.: 2024.1).");

        if (ordinal < Ordinal(Minimo))
            throw new ArgumentException($"O período letivo mais antigo permitido é {Minimo}.");

        if (TryGetOrdinal(periodoIngresso, out var ordIngresso) && ordinal < ordIngresso)
            throw new ArgumentException(
                $"Não é permitido registrar horas em um período anterior ao seu ingresso ({periodoIngresso!.Trim()}).");
    }
}
