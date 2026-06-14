using Back.Application.Interfaces.Identity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Back.Infrastructure.Services;

public class IdentityService : IIdentityService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public IdentityService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<(bool Success, string UserId, string[] Errors)> CreateUserAsync(string email, string password, string role)
    {
        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            return (false, "", result.Errors.Select(e => e.Description).ToArray());

        if (!await _roleManager.RoleExistsAsync(role))
            await _roleManager.CreateAsync(new IdentityRole(role));

        await _userManager.AddToRoleAsync(user, role);

        return (true, user.Id, Array.Empty<string>());
    }

    public async Task<(bool Success, string[] Errors)> UpdateUserAsync(string userId, string newEmail, string? newPassword)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return (false, new[] { "Usuário do Identity não encontrado." });
        }

        var errors = new List<string>();

        // 1. Atualizar Email e UserName (se mudou)
        if (!string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            var setEmailResult = await _userManager.SetEmailAsync(user, newEmail);
            if (!setEmailResult.Succeeded)
            {
                errors.AddRange(setEmailResult.Errors.Select(e => e.Description));
            }

            // Assumindo que UserName deve ser o mesmo que o Email
            var setUserNameResult = await _userManager.SetUserNameAsync(user, newEmail);
            if (!setUserNameResult.Succeeded)
            {
                errors.AddRange(setUserNameResult.Errors.Select(e => e.Description));
            }
        }

        // 2. Atualizar Senha (se fornecida)
        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            // Gera um token de reset e o utiliza para definir a nova senha.
            // Isso invalida a senha antiga sem precisar dela.
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetPasswordResult = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (!resetPasswordResult.Succeeded)
            {
                errors.AddRange(resetPasswordResult.Errors.Select(e => e.Description));
            }
        }

        if (errors.Any())
        {
            return (false, errors.ToArray());
        }

        return (true, Array.Empty<string>());
    }
}
