using Back.Application.DTOs.Campus;
using Back.Application.UseCases.Campus;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Back.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class CampusController : ControllerBase
{
    private readonly ListarCampusesUseCase _listar;
    private readonly CriarCampusUseCase _criar;

    public CampusController(ListarCampusesUseCase listar, CriarCampusUseCase criar)
    {
        _listar = listar;
        _criar = criar;
    }

    /// <summary>
    /// Lista todos os campi cadastrados.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista todos os campi.", Tags = new[] { "Campus" })]
    [ProducesResponseType(typeof(IEnumerable<CampusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodos()
    {
        var campi = await _listar.ExecuteAsync();
        return Ok(campi);
    }

    /// <summary>
    /// Cria um novo campus.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Cria um novo campus.", Tags = new[] { "Campus" })]
    [ProducesResponseType(typeof(CampusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Criar([FromBody] CreateCampusRequest request)
    {
        var campus = await _criar.ExecuteAsync(request);
        return Ok(campus);
    }
}
