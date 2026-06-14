using Back.Domain.Entities.Campus;
using Back.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Infrastructure.Seeders;

/// <summary>
/// Semeia os campi ativos do IFPE. É idempotente: só insere os que ainda não
/// existem. Belo Jardim é o primeiro da lista para ser o campus padrão
/// vinculado ao coordenador/curso criado no primeiro boot.
/// </summary>
public static class CampusSeeder
{
    public const string CidadePadrao = "Belo Jardim";

    private static readonly (string Nome, string Cidade)[] Campi =
    {
        ("Campus Belo Jardim",             "Belo Jardim"),
        ("Campus Abreu e Lima",            "Abreu e Lima"),
        ("Campus Afogados da Ingazeira",   "Afogados da Ingazeira"),
        ("Campus Barreiros",               "Barreiros"),
        ("Campus Cabo de Santo Agostinho", "Cabo de Santo Agostinho"),
        ("Campus Caruaru",                 "Caruaru"),
        ("Campus Garanhuns",               "Garanhuns"),
        ("Campus Igarassu",                "Igarassu"),
        ("Campus Ipojuca",                 "Ipojuca"),
        ("Campus Jaboatão dos Guararapes", "Jaboatão dos Guararapes"),
        ("Campus Olinda",                  "Olinda"),
        ("Campus Palmares",                "Palmares"),
        ("Campus Paulista",                "Paulista"),
        ("Campus Pesqueira",               "Pesqueira"),
        ("Campus Recife",                  "Recife"),
        ("Campus Vitória de Santo Antão",  "Vitória de Santo Antão"),
    };

    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var existentes = await context.Campi.Select(c => c.Nome).ToListAsync();

        foreach (var (nome, cidade) in Campi)
        {
            if (existentes.Contains(nome))
                continue;

            var campus = new CampusBuilder()
                .WithId(Guid.NewGuid())
                .WithNome(nome)
                .WithCidade(cidade)
                .Build();

            context.Campi.Add(campus);
        }

        await context.SaveChangesAsync();

        // Remove o "Campus Padrão" inserido pela migration de backfill quando a
        // tabela estava vazia, desde que não tenha cursos vinculados.
        var placeholders = await context.Campi
            .Where(c => c.Nome == "Campus Padrão" && !c.Cursos.Any())
            .ToListAsync();

        if (placeholders.Count > 0)
        {
            context.Campi.RemoveRange(placeholders);
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Retorna o campus padrão (Belo Jardim), garantindo que os campi foram semeados.
    /// </summary>
    public static async Task<Campus> ObterPadraoAsync(ApplicationDbContext context)
    {
        await SeedAsync(context);
        return await context.Campi.FirstAsync(c => c.Cidade == CidadePadrao);
    }
}
