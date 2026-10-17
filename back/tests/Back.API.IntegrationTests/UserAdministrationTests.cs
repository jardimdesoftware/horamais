using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Back.Application.DTOs.Auth;
using Back.Infrastructure.Persistence.Context;
using Back.Infrastructure.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Back.API.IntegrationTests;

[Collection("Integration")]
public class UserAdministrationTests(CustomWebApplicationFactory factory)
{
    private const string Password = "Transferencia@2026!";

    private async Task<(string Id, string Email)> CreateUserAsync(bool verified = true)
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var email = $"gestao-{Guid.NewGuid():N}@ifpe.edu.br";
        var user = new IdentityUser { Email = email, UserName = email, EmailConfirmed = verified };
        (await users.CreateAsync(user, Password)).Succeeded.Should().BeTrue();
        (await users.AddToRoleAsync(user, "USUARIO")).Succeeded.Should().BeTrue();
        return (user.Id, email);
    }

    private async Task<HttpClient> LoginAsync(string email, string password = Password)
    {
        var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, senha = password });
        response.EnsureSuccessStatusCode();
        var login = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
        return client;
    }

    private async Task<string> InitialIdAsync()
    {
        using var scope = factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        return (await users.FindByEmailAsync(CustomWebApplicationFactory.AdminEmail))!.Id;
    }

    [Fact]
    public async Task Admin_CriaCoordenadorSemSenha_ParaPrimeiroLoginGoogle()
    {
        Guid courseId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            courseId = await db.Cursos.Select(c => c.Id).FirstAsync();
        }
        var email = $"novo-coordenador-{Guid.NewGuid():N}@ifpe.edu.br";
        var admin = await factory.CreateAdminClientAsync();
        (await admin.PostAsJsonAsync("/api/usuarios/coordenador", new
        {
            email, cursoId = courseId, nome = "Nova Coordenadora",
            numeroPortaria = "456/2026", dou = "DOU 17/10/2026"
        })).StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var check = factory.Services.CreateScope();
        var users = check.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var dbCheck = check.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var user = (await users.FindByEmailAsync(email))!;
        user.Should().NotBeNull();
        (await users.HasPasswordAsync(user)).Should().BeFalse();
        (await users.IsInRoleAsync(user, "COORDENADOR")).Should().BeTrue();
        (await dbCheck.Coordenadores.SingleAsync(c => c.IdentityUserId == user.Id)).CursoId.Should().Be(courseId);
    }

    [Fact]
    public async Task Admin_AtribuiCoordenacaoAContaExistente()
    {
        var target = await CreateUserAsync();
        Guid courseId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            courseId = await db.Cursos.Select(c => c.Id).FirstAsync();
        }
        var request = new { cursoId = courseId, nome = "Coordenadora Teste", numeroPortaria = "123/2026", dou = "DOU 17/10/2026" };
        var normal = await LoginAsync(target.Email);
        (await normal.PutAsJsonAsync($"/api/usuarios/{target.Id}/coordenador", request))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var admin = await factory.CreateAdminClientAsync();
        (await admin.PutAsJsonAsync($"/api/usuarios/{target.Id}/coordenador", request))
            .StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await normal.GetAsync("/api/usuarios")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var updated = await LoginAsync(target.Email);
        var page = await admin.GetFromJsonAsync<UserList>($"/api/usuarios?search={target.Email}");
        page!.Items.Should().ContainSingle(x => x.Id == target.Id && x.Cargo == "COORDENADOR");
        using var check = factory.Services.CreateScope();
        var dbCheck = check.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await dbCheck.Coordenadores.SingleAsync(c => c.IdentityUserId == target.Id)).CursoId.Should().Be(courseId);
    }

    [Fact]
    public async Task Endpoints_ExigemAdmin_E_PromocaoExigeEmailVerificado()
    {
        var normal = await CreateUserAsync();
        var pending = await CreateUserAsync(false);
        var anonymous = factory.CreateClient();
        (await anonymous.GetAsync("/api/usuarios")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var normalClient = await LoginAsync(normal.Email);
        (await normalClient.GetAsync("/api/usuarios")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await normalClient.PatchAsJsonAsync($"/api/usuarios/{normal.Id}", new { administrador = true }))
            .StatusCode.Should().Be(HttpStatusCode.Forbidden);
        var admin = await factory.CreateAdminClientAsync();
        (await admin.PatchAsJsonAsync($"/api/usuarios/{pending.Id}", new { administrador = true }))
            .StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        var page = await admin.GetFromJsonAsync<UserList>($"/api/usuarios?search={normal.Email}");
        page!.Items.Should().ContainSingle(x => x.Id == normal.Id && x.Cargo == "USUARIO");
    }

    [Fact]
    public async Task Transferencia_ProtegeUltimoAdmin_E_RevogaSessoes()
    {
        var initialId = await InitialIdAsync();
        var admin = await factory.CreateAdminClientAsync();
        (await admin.PatchAsJsonAsync($"/api/usuarios/{initialId}", new { administrador = false }))
            .StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        (await admin.PatchAsJsonAsync($"/api/usuarios/{initialId}", new { ativo = false }))
            .StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

        var successor = await CreateUserAsync();
        (await admin.PatchAsJsonAsync($"/api/usuarios/{successor.Id}", new { administrador = true })).EnsureSuccessStatusCode();
        var nextAdmin = await LoginAsync(successor.Email);
        try
        {
            (await admin.PatchAsJsonAsync($"/api/usuarios/{initialId}", new { administrador = false })).EnsureSuccessStatusCode();
            (await admin.GetAsync("/api/usuarios")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
            var formerAdmin = await factory.CreateAdminClientAsync();
            (await formerAdmin.GetAsync("/api/usuarios")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
            (await nextAdmin.PatchAsJsonAsync($"/api/usuarios/{successor.Id}", new { ativo = false }))
                .StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
            (await nextAdmin.PatchAsJsonAsync($"/api/usuarios/{initialId}", new { administrador = true })).EnsureSuccessStatusCode();
        }
        finally
        {
            // Restore the shared fixture even when an assertion fails.
            using var scope = factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<UserAdministrationService>();
            await service.UpdateAsync(successor.Id, initialId, true, true);
            await service.UpdateAsync(initialId, successor.Id, true, false);
        }
        admin = await factory.CreateAdminClientAsync();
        var userClient = await LoginAsync(successor.Email);
        (await admin.PatchAsJsonAsync($"/api/usuarios/{successor.Id}", new { ativo = false })).EnsureSuccessStatusCode();
        (await userClient.GetAsync("/api/usuarios")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var rejected = await factory.CreateClient().PostAsJsonAsync("/api/auth/login", new { email = successor.Email, senha = Password });
        rejected.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task DesativacoesConcorrentes_NaoRemovemTodosOsAdmins()
    {
        var initialId = await InitialIdAsync();
        var successor = await CreateUserAsync();
        var admin = await factory.CreateAdminClientAsync();
        (await admin.PatchAsJsonAsync($"/api/usuarios/{successor.Id}", new { administrador = true })).EnsureSuccessStatusCode();
        var nextAdmin = await LoginAsync(successor.Email);
        try
        {
            var responses = await Task.WhenAll(
                admin.PatchAsJsonAsync($"/api/usuarios/{initialId}", new { ativo = false }),
                nextAdmin.PatchAsJsonAsync($"/api/usuarios/{successor.Id}", new { ativo = false }));
            responses.Count(r => r.IsSuccessStatusCode).Should().Be(1);
            responses.Count(r => r.StatusCode == HttpStatusCode.UnprocessableEntity).Should().Be(1);
        }
        finally
        {
            using var scope = factory.Services.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<UserAdministrationService>();
            var remaining = await service.IsActiveAsync(initialId) ? initialId : successor.Id;
            await service.UpdateAsync(remaining, initialId, true, true);
            await service.UpdateAsync(initialId, successor.Id, true, false);
        }
    }

    [Fact]
    public async Task Bootstrap_PromoveSomenteEmailConfigurado_UmaUnicaVez()
    {
        var email = $"bootstrap-{Guid.NewGuid():N}@discente.ifpe.edu.br";
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
            { ["INITIAL_ADMIN_EMAIL"] = email }).Build();
        string id;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var service = new UserAdministrationService(db, users, config);
            await service.BootstrapVerifiedEmailAsync("outra-conta@discente.ifpe.edu.br");
            (await users.FindByEmailAsync(email)).Should().BeNull();
            await service.BootstrapVerifiedEmailAsync(email.ToUpperInvariant());
            var user = (await users.FindByEmailAsync(email))!;
            id = user.Id;
            user.EmailConfirmed.Should().BeTrue();
            (await users.HasPasswordAsync(user)).Should().BeFalse();
            (await users.IsInRoleAsync(user, "ADMIN")).Should().BeTrue();
            await service.UpdateAsync(await InitialIdAsync(), id, true, false);
        }
        try
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var users = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var service = new UserAdministrationService(db, users, config);
            await service.BootstrapVerifiedEmailAsync(email);
            (await users.IsInRoleAsync((await users.FindByIdAsync(id))!, "ADMIN")).Should().BeFalse();
            (await db.AdminInitializations.CountAsync()).Should().Be(1);
        }
        finally
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.AdminInitializations.ExecuteDeleteAsync();
        }
    }

    private record UserList(ManagedUser[] Items, int Total);
}
