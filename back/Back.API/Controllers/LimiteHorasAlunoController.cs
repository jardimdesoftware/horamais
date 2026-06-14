using Back.Application.DTOs.LimiteHorasAluno;
using Back.Application.UseCases.LimiteHorasAluno;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Back.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class LimiteHorasAlunoController : ControllerBase
{
    private readonly CreateLimiteHorasAlunoUseCase _create;

    public LimiteHorasAlunoController(CreateLimiteHorasAlunoUseCase create)
    {
        _create = create;
    }

    /// <summary>
    /// Cadastra os limites de horas para um curso.
    /// </summary>
    /// <remarks>Somente ADMIN pode cadastrar os limites de horas.</remarks>
    /// <param name="request">Dados com os limites máximos de horas complementares e de extensão.</param>
    /// <returns>ID do registro criado.</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Cria limites de horas por curso",
        Description = "Permite definir a carga horária máxima complementar e de extensão para um curso.",
        Tags = new[] { "Limites de Horas" })]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> Criar([FromBody] CreateLimiteHorasAlunoRequest request)
    {
        var id = await _create.ExecuteAsync(request);
        return CreatedAtAction(nameof(Criar), new { id }, new { id });
    }
}
