using Back.Domain.Entities.Campus;
using Back.Domain.Entities.Coordenador;
using Back.Domain.Entities.Curso;
using Back.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Infrastructure.Seeders;

public class CoordenadorSeeder
{
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

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null) return;

        // Garante que exista um campus para vincular ao curso
        var campus = await context.Campi.FirstOrDefaultAsync();
        if (campus == null)
        {
            var campusNome = Environment.GetEnvironmentVariable("CAMPUS_NOME") ?? "Campus Padrão";
            var campusCidade = Environment.GetEnvironmentVariable("CAMPUS_CIDADE") ?? "Cidade";
            campus = new CampusBuilder()
                .WithId(Guid.NewGuid())
                .WithNome(campusNome)
                .WithCidade(campusCidade)
                .Build();
            context.Campi.Add(campus);
            await context.SaveChangesAsync();
        }

        // Garante que exista um curso para vincular
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
}
