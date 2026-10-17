using System.Security.Claims;
using Back.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.API.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "ADMIN")]
public class UsuariosController(UserAdministrationService administration) : ControllerBase
{
    public record UpdateUserRequest(bool? Ativo, bool? Administrador);
    public record AssignCoordinatorRequest(Guid CursoId, string Nome, string NumeroPortaria, string Dou);
    public record CreateCoordinatorRequest(string Email, Guid CursoId, string Nome, string NumeroPortaria, string Dou);

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? search, [FromQuery] int page = 1)
        => Ok(await administration.ListAsync(search, page));

    [HttpPatch("{id}")]
    public async Task<IActionResult> Update(string id, UpdateUserRequest request)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await administration.UpdateAsync(actor, id, request.Ativo, request.Administrador);
        return NoContent();
    }

    [HttpPut("{id}/coordenador")]
    public async Task<IActionResult> AssignCoordinator(string id, AssignCoordinatorRequest request)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await administration.AssignCoordinatorAsync(actor, id, request.CursoId, request.Nome,
            request.NumeroPortaria, request.Dou);
        return NoContent();
    }

    [HttpPost("coordenador")]
    public async Task<IActionResult> CreateCoordinator(CreateCoordinatorRequest request)
    {
        var actor = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await administration.CreateCoordinatorAsync(actor, request.Email, request.CursoId, request.Nome,
            request.NumeroPortaria, request.Dou);
        return NoContent();
    }
}
