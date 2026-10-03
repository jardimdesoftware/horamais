using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Back.Domain.Entities.Aluno;
using Back.Domain.Entities.AlunoAtividade;
using Back.Domain.Entities.Campus;
using Back.Domain.Entities.Coordenador;
using Back.Domain.Entities.Curso;
using Back.Domain.Entities.LimiteHorasAluno;
using Back.Domain.Entities.Turma;
using Back.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Back.Infrastructure.Seeders;

public static class DevDataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        if (await context.Cursos.AnyAsync())
            return;

        // Campi do IFPE (semeados pelo CampusSeeder); usa Belo Jardim para o curso.
        var campusPadrao = await CampusSeeder.ObterPadraoAsync(context);
        var campusId = campusPadrao.Id;

        // Curso
        var cursoId = Guid.NewGuid();
        var curso = new CursoBuilder()
            .WithId(cursoId)
            .WithNome("Análise e Desenvolvimento de Sistemas")
            .WithCampusId(campusId)
            .Build();

        context.Cursos.Add(curso);
        await context.SaveChangesAsync();

        // Limite de horas do curso
        var limite = new LimiteHorasAlunoBuilder()
            .WithId(Guid.NewGuid())
            .WithCursoId(cursoId)
            .WithMaximoHorasComplementar(120)
            .Build();

        context.LimitesHoras.Add(limite);
        await context.SaveChangesAsync();

        // Turmas
        var turma1Id = Guid.NewGuid();
        var turma1 = new TurmaBuilder()
            .WithId(turma1Id)
            .WithPeriodo("2024.1")
            .WithTurno("noite")
            .WithCodigo("ADS1B7")
            .WithCursoId(cursoId)
            .WithPossuiExtensao(true)
            .WithMaximoHorasExtensao(80)
            .Build();

        var turma2Id = Guid.NewGuid();
        var turma2 = new TurmaBuilder()
            .WithId(turma2Id)
            .WithPeriodo("2024.2")
            .WithTurno("manha")
            .WithCodigo("ADS2B7")
            .WithCursoId(cursoId)
            .WithPossuiExtensao(false)
            .Build();

        context.Turmas.AddRange(turma1, turma2);
        await context.SaveChangesAsync();

        Console.WriteLine(" Dev data seed concluído.");
    }
}
