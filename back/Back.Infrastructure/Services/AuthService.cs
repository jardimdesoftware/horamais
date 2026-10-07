using Back.Application.DTOs.Auth;
using Back.Application.Interfaces.Identity;
using Back.Infrastructure.Persistence.Context;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Back.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _config;
    private readonly ApplicationDbContext _context;

    public AuthService(
        UserManager<IdentityUser> userManager,
        IConfiguration config,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _config = config;
        _context = context;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var identityUser = await _userManager.FindByEmailAsync(dto.Email);
        if (identityUser == null || await _userManager.IsLockedOutAsync(identityUser))
            throw new UnauthorizedAccessException("Email ou senha inválidos.");

        if (await _userManager.IsInRoleAsync(identityUser, "ALUNO"))
            throw new UnauthorizedAccessException("Email ou senha inválidos.");

        if (!identityUser.LockoutEnabled)
        {
            identityUser.LockoutEnabled = true;
            await _userManager.UpdateAsync(identityUser);
        }

        if (!await _userManager.CheckPasswordAsync(identityUser, dto.Senha))
        {
            await _userManager.AccessFailedAsync(identityUser);
            throw new UnauthorizedAccessException("Email ou senha inválidos.");
        }

        await _userManager.ResetAccessFailedCountAsync(identityUser);

        if (!identityUser.EmailConfirmed)
            throw new UnauthorizedAccessException("E-mail não verificado. Verifique sua caixa de entrada para confirmar o código de cadastro.");

        return await BuildLoginResponseAsync(identityUser);
    }

    public async Task<GoogleLoginResponseDto> LoginWithGoogleAsync(GoogleLoginRequestDto dto)
    {
        var clientId = _config["Authentication:Google:ClientId"];
        if (string.IsNullOrWhiteSpace(clientId))
            throw new UnauthorizedAccessException("Login com Google não configurado.");

        GoogleJsonWebSignature.Payload payload;
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId }
            };
            payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
        }
        catch (Exception)
        {
            throw new UnauthorizedAccessException("Token do Google inválido ou expirado.");
        }

        if (!payload.EmailVerified)
            throw new UnauthorizedAccessException("E-mail do Google não verificado.");

        var identityUser = await _userManager.FindByEmailAsync(payload.Email);
        if (identityUser == null)
            return GoogleLoginResponseDto.ForFirstAccess(payload.Name, payload.Email, CreateGoogleRegistrationTicket(payload.Email));

        if (!identityUser.EmailConfirmed)
        {
            var roles = await _userManager.GetRolesAsync(identityUser);
            if (roles.Contains("ALUNO") && await _context.Alunos.AnyAsync(a => a.IdentityUserId == identityUser.Id))
            {
                if (await _userManager.HasPasswordAsync(identityUser))
                {
                    var removePassword = await _userManager.RemovePasswordAsync(identityUser);
                    if (!removePassword.Succeeded)
                        throw new InvalidOperationException("Não foi possível concluir o cadastro com Google.");
                }
                identityUser.EmailConfirmed = true;
                var update = await _userManager.UpdateAsync(identityUser);
                if (!update.Succeeded)
                    throw new InvalidOperationException("Não foi possível concluir o cadastro com Google.");
            }
            else
                return GoogleLoginResponseDto.ForPendingVerification(payload.Name, payload.Email);
        }

        var login = await BuildLoginResponseAsync(identityUser);
        return new GoogleLoginResponseDto(login.Nome, login.Email, login.Role, login.Token);
    }

    private byte[] GoogleRegistrationSigningKey()
    {
        var jwtKey = _config["JWT:Key"] ?? throw new InvalidOperationException("Chave JWT não configurada.");
        return SHA256.HashData(Encoding.UTF8.GetBytes("google-registration:" + jwtKey));
    }

    private string CreateGoogleRegistrationTicket(string email)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(new GoogleRegistrationTicket(email, DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds()));
        var signature = HMACSHA256.HashData(GoogleRegistrationSigningKey(), payload);
        return Base64UrlEncoder.Encode(payload) + "." + Base64UrlEncoder.Encode(signature);
    }

    public string ValidateGoogleRegistrationTicket(string ticket)
    {
        if (string.IsNullOrWhiteSpace(ticket))
            throw new UnauthorizedAccessException("Cadastro com Google expirado. Entre com Google novamente.");

        try
        {
            var parts = ticket.Split('.');
            if (parts.Length != 2) throw new FormatException();
            var payload = Base64UrlEncoder.DecodeBytes(parts[0]);
            var signature = Base64UrlEncoder.DecodeBytes(parts[1]);
            var expected = HMACSHA256.HashData(GoogleRegistrationSigningKey(), payload);
            if (signature.Length != expected.Length || !CryptographicOperations.FixedTimeEquals(signature, expected))
                throw new FormatException();
            var claims = JsonSerializer.Deserialize<GoogleRegistrationTicket>(payload);
            if (claims == null || claims.ExpiresAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds() || string.IsNullOrWhiteSpace(claims.Email))
                throw new FormatException();
            return claims.Email;
        }
        catch (Exception ex) when (ex is FormatException or JsonException or ArgumentException)
        {
            throw new UnauthorizedAccessException("Cadastro com Google expirado. Entre com Google novamente.");
        }
    }

    private sealed record GoogleRegistrationTicket(string Email, long ExpiresAt);

    private async Task<LoginResponseDto> BuildLoginResponseAsync(IdentityUser identityUser)
    {
        var roles = await _userManager.GetRolesAsync(identityUser);
        var role = roles.FirstOrDefault() ?? throw new UnauthorizedAccessException("Usuário sem perfil.");

        string nome;
        Guid entidadeId;
        Guid cursoId = Guid.Empty;
        Guid turmaId = Guid.Empty;
        bool isNewPpc = false;

        switch (role.ToUpper())
        {
            case "ALUNO":
                var aluno = await _context.Alunos
                    .Include(a => a.Turma)
                    .ThenInclude(t => t.Curso)
                    .FirstOrDefaultAsync(a => a.IdentityUserId == identityUser.Id)
                    ?? throw new UnauthorizedAccessException("Aluno não encontrado.");

                if (!aluno.IsAtivo)
                    throw new UnauthorizedAccessException("Aluno inativo. Acesso não permitido.");

                var turma = aluno.Turma ?? throw new UnauthorizedAccessException("Curso não encontrado.");
                nome = aluno.Nome;
                entidadeId = aluno.Id;
                turmaId = aluno.TurmaId;
                cursoId = turma.CursoId;
                isNewPpc = turma.PossuiExtensao;
                break;

            case "COORDENADOR":
                var coordenador = await _context.Coordenadores
                    .FirstOrDefaultAsync(c => c.IdentityUserId == identityUser.Id)
                    ?? throw new UnauthorizedAccessException("Coordenador não encontrado.");

                nome = coordenador.Nome;
                entidadeId = coordenador.Id;
                cursoId = coordenador.CursoId;
                break;

            case "ADMIN":
                var admin = await _context.Admins
                    .FirstOrDefaultAsync(a => a.IdentityUserId == identityUser.Id)
                    ?? throw new UnauthorizedAccessException("Admin não encontrado.");

                nome = admin.Email;
                entidadeId = admin.Id;
                break;

            default:
                throw new UnauthorizedAccessException("Perfil inválido.");
        }

        var token = GenerateJwtToken(identityUser, role, nome, entidadeId, cursoId, turmaId, isNewPpc);

        return new LoginResponseDto(nome, identityUser.Email!, role, token);
    }

    private string GenerateJwtToken(IdentityUser user, string role, string nome, Guid entidadeId, Guid cursoId, Guid turmaId, bool isNewPpc)
    {
        var expiration = DateTime.UtcNow.AddHours(4);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim("nome", nome),
            new Claim("entidadeId", entidadeId.ToString()),
            new Claim("security_stamp", user.SecurityStamp ?? string.Empty),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Exp, new DateTimeOffset(expiration).ToUnixTimeSeconds().ToString())
        };

        if (cursoId != Guid.Empty)
            claims.Add(new Claim("cursoId", cursoId.ToString()));

        if (turmaId != Guid.Empty)
            claims.Add(new Claim("turmaId", turmaId.ToString()));
        if (role.ToUpper() == "ALUNO")
            claims.Add(new Claim("isNewPpc", isNewPpc.ToString().ToLower()));
        var jwtKey = _config["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(jwtKey))
            throw new Exception("A chave JWT (Jwt:Key) não foi configurada corretamente. Verifique o .env ou appsettings.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
