using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;

namespace Back.Infrastructure.Seeders;

/// <summary>Invalidates the shared password shipped by old development fixtures.</summary>
public static class LegacySeedCredentialRemediator
{
    private static readonly string[] FixtureEmails =
    [
        "coordenador.ads@ifpe.edu.br",
        "20230001.ads@ifpe.edu.br",
        "20230002.ads@ifpe.edu.br",
        "20230003.ads@ifpe.edu.br",
        "20240001.ads@ifpe.edu.br",
        "20240002.ads@ifpe.edu.br"
    ];

    public static async Task RunAsync(UserManager<IdentityUser> userManager)
    {
        foreach (var email in FixtureEmails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user is null || !await userManager.HasPasswordAsync(user) ||
                !await userManager.CheckPasswordAsync(user, "Senha@123"))
                continue;

            var result = await userManager.RemovePasswordAsync(user);
            if (!result.Succeeded)
                throw new InvalidOperationException($"Falha ao invalidar credencial de seed para {email}.");
        }
    }
}
