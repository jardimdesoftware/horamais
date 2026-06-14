using Back.Application.DTOs.Curso;
using Back.Application.UseCases.Curso;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Back.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "ADMIN")]
public class CursoController : ControllerBase
{
    private readonly CreateCursoUseCase _create;
    private readonly GetAllCursosUseCase _getAll;
    private readonly GetCursoByIdUseCase _getById;
    private readonly GetResumoCursosUseCase _getResumo;
    private readonly UpdateCursoUseCase _update;
    private readonly DeleteCursoUseCase _delete;
    public CursoController(
        CreateCursoUseCase create,
        GetAllCursosUseCase getAll,
        GetCursoByIdUseCase getById,
        GetResumoCursosUseCase getResumo,
        UpdateCursoUseCase update, 
        DeleteCursoUseCase delete)
    {
        _create = create;
        _getAll = getAll;
        _getById = getById;
        _getResumo = getResumo;
        _update = update; 
        _delete = delete; 
    }

    /// <summary>
    /// Cria um novo curso.
    /// </summary>
    /// <remarks>Requer perfil ADMIN.</remarks>
    /// <param name="request">Dados do novo curso</param>
    /// <returns>Curso criado</returns>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Cria um novo curso.",
        Description = "Somente usuários com perfil ADMIN podem cadastrar um novo curso.",
        Tags = new[] { "Cursos" })]
    [ProducesResponseType(typeof(CursoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CreateCursoComLimiteHorasRequest request)
    {
        try
        {
            var curso = await _create.ExecuteAsync(request);
            return CreatedAtAction(nameof(ObterPorId), new { id = curso.Id }, curso);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    /// <summary>
    /// Lista todos os cursos.
    /// </summary>
    /// <remarks>Requer perfil ADMIN.</remarks>
    /// <returns>Lista de cursos</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lista todos os cursos cadastrados.",
        Tags = new[] { "Cursos" })]
    [ProducesResponseType(typeof(IEnumerable<CursoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodos([FromQuery] Guid? campusId = null)
    {
        var cursos = await _getAll.ExecuteAsync(campusId);
        return Ok(cursos);
    }

    /// <summary>
    /// Obtém os detalhes de um curso pelo ID.
    /// </summary>
    /// <remarks>Requer perfil ADMIN.</remarks>
    /// <param name="id">ID do curso</param>
    /// <returns>Curso encontrado</returns>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(
        Summary = "Busca um curso pelo ID.",
        Tags = new[] { "Cursos" })]
    [ProducesResponseType(typeof(CursoDetalhadoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        try
        {
            var curso = await _getById.ExecuteAsync(id);
            return Ok(curso);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
    }

    /// <summary>
    /// Atualiza um curso e seus limites de horas.
    /// </summary>
    /// <remarks>Requer perfil ADMIN.</remarks>
    /// <param name="id">ID do curso a ser atualizado.</param>
    /// <param name="request">Novos dados do curso e limites.</param>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Atualiza um curso existente.", Tags = new[] { "Cursos" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] UpdateCursoComLimiteHorasRequest request)
    {
        try
        {
            await _update.ExecuteAsync(id, request);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        catch (InvalidOperationException ex) // Captura erro de dados inconsistentes
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    /// <summary>
    /// Remove um curso e TODOS os dados associados.
    /// </summary>
    /// <remarks>
    /// Requer perfil ADMIN. AÇÃO PERMANENTE E DESTRUTIVA.
    /// Remove Turmas, Alunos, Certificados, Atividades e Coordenador deste curso.
    /// </remarks>
    /// <param name="id">ID do curso a ser removido.</param>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Remove um curso e todos os seus dados (Cascata).", Tags = new[] { "Cursos" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deletar(Guid id)
    {
        try
        {
            await _delete.ExecuteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { erro = ex.Message });
        }
        // Você pode querer tratar exceções de FK aqui, mas o UseCase deve
        // lidar com a ordem correta.
    }

    /// <summary>
    /// Retorna resumo de todos os cursos com quantidade de turmas e alunos.
    /// </summary>
    [HttpGet("resumo")]
    [SwaggerOperation(Summary = "Resumo dos cursos (turmas e alunos)", Tags = new[] { "Cursos" })]
    [ProducesResponseType(typeof(IEnumerable<CursoResumoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ResumoCursos()
    {
        var resumo = await _getResumo.ExecuteAsync();
        return Ok(resumo);
    }

}
