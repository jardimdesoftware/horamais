using Back.Domain.Entities.Campus;
using Back.Domain.Entities.Coordenador;
using Back.Domain.Entities.Curso;
using Back.Domain.Entities.LimiteHorasAluno;
using Back.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Infrastructure.Seeders;

public class CoordenadorSeeder
{
    // Padrão usado quando CURSO_MAX_HORAS_COMPLEMENTAR não está definido.
    private const int MaximoHorasComplementarPadrao = 120;

    public static async Task SeedAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        var email = Environment.GetEnvironmentVariable("COORD_EMAIL");
        var senha = Environment.GetEnvironmentVariable("COORD_PASSWORD");
        var nome = Environment.GetEnvironmentVariable("COORD_NOME") ?? "Coordenador Geral";
        var portaria = Environment.GetEnvironmentVariable("COORD_PORTARIA") ?? "000/0000";
        var dou = Environment.GetEnvironmentVariable("COORD_DOU") ?? DateTime.Now.ToString("yyyy-MM-dd");
        var cursoNome = Environment.GetEnvironmentVariable("CURSO_NOME") ?? "Curso Padrão";

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            return; // Se não houver configuração, não cria (opcional, diferente do admin que é obrigatório)

        // Vincula ao campus padrão (Belo Jardim), garantido pelo CampusSeeder.
        var campus = await CampusSeeder.ObterPadraoAsync(context);

        // Garante que exista um curso para vincular.
        var curso = await context.Cursos.FirstOrDefaultAsync(c => c.Nome == cursoNome);
        if (curso == null)
        {
            curso = new CursoBuilder()
                .WithId(Guid.NewGuid())
                .WithNome(cursoNome)
                .WithCampusId(campus.Id)
                .Build();
            context.Cursos.Add(curso);
            await context.SaveChangesAsync();
        }

        // Garante o limite de horas complementares do curso (idempotente). Sem ele,
        // o cálculo de conclusão/risco do aluno não funciona, pois esse máximo é a
        // base do percentual. Roda a cada startup, corrigindo cursos antigos.
        var temLimite = await context.LimitesHoras.AnyAsync(l => l.CursoId == curso.Id);
        if (!temLimite)
        {
            var limite = new LimiteHorasAlunoBuilder()
                .WithId(Guid.NewGuid())
                .WithCursoId(curso.Id)
                .WithMaximoHorasComplementar(ResolverMaximoHorasComplementar())
                .Build();
            context.LimitesHoras.Add(limite);
            await context.SaveChangesAsync();
        }

        // Cria o coordenador apenas se ainda não existir.
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null) return;

        var identityUser = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(identityUser, senha);
        if (!result.Succeeded)
            throw new Exception("Erro ao criar coordenador: " + string.Join("; ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(identityUser, "COORDENADOR");

        var coordenador = new CoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithNome(nome)
            .WithNumeroPortaria(portaria)
            .WithDOU(dou)
            .WithEmail(email)
            .WithCursoId(curso.Id)
            .WithIdentityUserId(identityUser.Id)
            .Build();

        context.Coordenadores.Add(coordenador);
        await context.SaveChangesAsync();
    }

    private static int ResolverMaximoHorasComplementar()
    {
        var raw = Environment.GetEnvironmentVariable("CURSO_MAX_HORAS_COMPLEMENTAR");
        return int.TryParse(raw, out var valor) && valor > 0
            ? valor
            : MaximoHorasComplementarPadrao;
    }
}
