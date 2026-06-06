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
        await SeedCoordenadorAsync(context, userManager);

        if (await context.Cursos.AnyAsync())
            return;

        // Atividades globais (antes de cursos/alunos para poder vincular)
        await AtividadeSeeder.SeedAsync(context);

        // Campi do IFPE
        var campiData = new[]
        {
            ("Campus Abreu e Lima",              "Abreu e Lima"),
            ("Campus Afogados da Ingazeira",     "Afogados da Ingazeira"),
            ("Campus Barreiros",                 "Barreiros"),
            ("Campus Belo Jardim",               "Belo Jardim"),
            ("Campus Cabo de Santo Agostinho",   "Cabo de Santo Agostinho"),
            ("Campus Caruaru",                   "Caruaru"),
            ("Campus Garanhuns",                 "Garanhuns"),
            ("Campus Igarassu",                  "Igarassu"),
            ("Campus Ipojuca",                   "Ipojuca"),
            ("Campus Jaboatão dos Guararapes",   "Jaboatão dos Guararapes"),
            ("Campus Olinda",                    "Olinda"),
            ("Campus Palmares",                  "Palmares"),
            ("Campus Paulista",                  "Paulista"),
            ("Campus Pesqueira",                 "Pesqueira"),
            ("Campus Recife",                    "Recife"),
            ("Campus Vitória de Santo Antão",    "Vitória de Santo Antão"),
        };

        Guid campusId = Guid.Empty;
        foreach (var (nome, cidade) in campiData)
        {
            var id = Guid.NewGuid();
            var campus = new CampusBuilder()
                .WithId(id)
                .WithNome(nome)
                .WithCidade(cidade)
                .Build();
            context.Campi.Add(campus);

            if (cidade == "Belo Jardim")
                campusId = id;
        }

        await context.SaveChangesAsync();

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

        // Coordenador
        var coordEmail = "coordenador.ads@ifpe.edu.br";
        var coordIdentity = new IdentityUser { UserName = coordEmail, Email = coordEmail, EmailConfirmed = true };
        var coordResult = await userManager.CreateAsync(coordIdentity, "Senha@123");
        if (coordResult.Succeeded)
            await userManager.AddToRoleAsync(coordIdentity, "COORDENADOR");

        var coordenador = new CoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("Prof. Carlos Mendonça")
            .WithNumeroPortaria("001/2024")
            .WithDOU("2024-01-15")
            .WithEmail(coordEmail)
            .WithCursoId(cursoId)
            .WithIdentityUserId(coordIdentity.Id)
            .Build();

        context.Coordenadores.Add(coordenador);
        await context.SaveChangesAsync();

        // Busca TODAS as atividades globais para vincular aos alunos
        var atividades = await context.Atividades.ToListAsync();

        // Alunos (turma 1)
        var alunosTurma1 = new[]
        {
            ("Ana Souza",    "20230001.ads@ifpe.edu.br", "20230001"),
            ("Bruno Lima",   "20230002.ads@ifpe.edu.br", "20230002"),
            ("Carlos Melo",  "20230003.ads@ifpe.edu.br", "20230003"),
        };

        // Alunos (turma 2)
        var alunosTurma2 = new[]
        {
            ("Diana Rocha",  "20240001.ads@ifpe.edu.br", "20240001"),
            ("Eduardo Paz",  "20240002.ads@ifpe.edu.br", "20240002"),
        };

        foreach (var (turmaId, alunos) in new[] { (turma1Id, alunosTurma1), (turma2Id, alunosTurma2) })
        {
            foreach (var (nome, email, matricula) in alunos)
            {
                var identityUser = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };
                var result = await userManager.CreateAsync(identityUser, "Senha@123");
                if (!result.Succeeded)
                    continue;

                await userManager.AddToRoleAsync(identityUser, "ALUNO");

                var alunoId = Guid.NewGuid();
                var aluno = new AlunoBuilder()
                    .WithId(alunoId)
                    .WithNome(nome)
                    .WithEmail(email)
                    .WithMatricula(matricula)
                    .WithTurmaId(turmaId)
                    .WithIdentityUserId(identityUser.Id)
                    .Build();

                context.Alunos.Add(aluno);

                var alunoAtividades = atividades.Select(a => new AlunoAtividadeBuilder()
                    .WithId(Guid.NewGuid())
                    .WithAlunoId(alunoId)
                    .WithAtividadeId(a.Id)
                    .WithHorasConcluidas(0)
                    .Build()).ToList();

                context.AlunoAtividades.AddRange(alunoAtividades);
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine(" Dev data seed concluído.");
    }

    private static async Task SeedCoordenadorAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        const string coordEmail = "coordenador.ads@ifpe.edu.br";
        const string coordSenha = "Senha@123";

        // Já existe → nada a fazer
        if (await userManager.FindByEmailAsync(coordEmail) != null)
            return;

        var curso = await context.Cursos.FirstOrDefaultAsync();
        if (curso == null)
            return; // cursos ainda não existem, o bloco abaixo vai criar junto

        var identity = new IdentityUser { UserName = coordEmail, Email = coordEmail, EmailConfirmed = true };
        var result = await userManager.CreateAsync(identity, coordSenha);
        if (!result.Succeeded)
        {
            Console.WriteLine($" Falha ao criar coordenador de dev: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            return;
        }

        await userManager.AddToRoleAsync(identity, "COORDENADOR");

        var coordenador = new CoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithNome("Prof. Carlos Mendonça")
            .WithNumeroPortaria("001/2024")
            .WithDOU("2024-01-15")
            .WithEmail(coordEmail)
            .WithCursoId(curso.Id)
            .WithIdentityUserId(identity.Id)
            .Build();

        context.Coordenadores.Add(coordenador);
        await context.SaveChangesAsync();
        Console.WriteLine(" Coordenador de dev criado.");
    }
}
