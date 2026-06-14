using Back.Domain.Entities.Admin;
using Back.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Infrastructure.Seeders;

public class AdminSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, UserManager<IdentityUser> userManager)
    {
        var email = Environment.GetEnvironmentVariable("ADMIN_EMAIL");
        var senha = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(senha))
            throw new Exception("ADMIN_EMAIL ou ADMIN_PASSWORD não configurado no .env");

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
            throw new Exception("Erro ao criar admin: " + string.Join("; ", result.Errors.Select(e => e.Description)));

        await userManager.AddToRoleAsync(identityUser, "ADMIN");

        var admin = new AdminBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail(email)
            .WithIdentityUserId(identityUser.Id)
            .Build();

        context.Admins.Add(admin);
        await context.SaveChangesAsync();
    }
}
