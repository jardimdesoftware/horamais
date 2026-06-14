using Back.Application.DTOs.Auth;
using Back.Application.Interfaces.Identity;
using Back.Infrastructure.Persistence.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
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
        if (identityUser == null || !await _userManager.CheckPasswordAsync(identityUser, dto.Senha))
            throw new UnauthorizedAccessException("Email ou senha inválidos.");

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
