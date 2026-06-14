using Back.Application.DTOs.Atividade;
using Back.Application.UseCases.Atividade;
using Back.Application.UseCases.Turma;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Back.API.Controllers;

/// <summary>
/// Controlador para operações relacionadas às atividades.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AtividadeController : ControllerBase
{
    private readonly GetAllAtividadesUseCase _getAll;
    private readonly CreateAtividadeUseCase _create;
    private readonly UpdateAtividadeUseCase _update;
    private readonly DeleteAtividadeUseCase _delete;
    private readonly GetPeriodosLetivosUseCase _getPeriodos;

    /// <summary>
    /// Inicializa o controlador de atividades com as dependências necessárias.
    /// </summary>
    public AtividadeController(
        GetAllAtividadesUseCase getAll,
        CreateAtividadeUseCase create,
        UpdateAtividadeUseCase update,
        DeleteAtividadeUseCase delete,
        GetPeriodosLetivosUseCase getPeriodos)
    {
        _getAll = getAll;
        _create = create;
        _update = update;
        _delete = delete;
        _getPeriodos = getPeriodos;
    }

    /// <summary>
    /// Retorna os períodos letivos distintos de todas as turmas cadastradas.
    /// </summary>
    [HttpGet("periodos")]
    [ProducesResponseType(typeof(IEnumerable<string>), 200)]
    public async Task<IActionResult> ListarPeriodos()
    {
        var periodos = await _getPeriodos.ExecuteAsync();
        return Ok(periodos);
    }

    /// <summary>
    /// Lista todas as atividades globais do sistema.
    /// </summary>
    /// <returns>Lista de atividades.</returns>
    /// <response code="200">Retorna a lista de atividades.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AtividadeResponse>), 200)]
    public async Task<IActionResult> Listar()
    {
        var atividades = await _getAll.ExecuteAsync();
        return Ok(atividades);
    }

    /// <summary>
    /// Cria uma nova atividade global (vinculada a todos os alunos).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "COORDENADOR")]
    [ProducesResponseType(typeof(object), 201)]
    public async Task<IActionResult> Criar([FromBody] CreateAtividadeRequest request)
    {
        var id = await _create.ExecuteAsync(request);
        return CreatedAtAction(nameof(Listar), new { id }, new { atividadeId = id });
    }

    /// <summary>
    /// Atualiza os dados de uma atividade.
    /// </summary>
    /// <remarks>Requer permissão de COORDENADOR.</remarks>
    /// <param name="id">ID da atividade a ser atualizada.</param>
    /// <param name="request">Novos dados da atividade.</param>
    /// <response code="204">Atividade atualizada com sucesso.</response>
    /// <response code="404">Atividade não encontrada.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "COORDENADOR")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] UpdateAtividadeRequest request)
    {
        await _update.ExecuteAsync(id, request);
        return NoContent();
    }

    /// <summary>
    /// Remove uma atividade e todos os seus vínculos com alunos.
    /// </summary>
    /// <remarks>Requer permissão de COORDENADOR. Esta ação é permanente.</remarks>
    /// <param name="id">ID da atividade a ser removida.</param>
    /// <response code="204">Atividade removida com sucesso.</response>
    /// <response code="404">Atividade não encontrada.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "COORDENADOR")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Deletar(Guid id)
    {
        await _delete.ExecuteAsync(id);
        return NoContent();
    }
}
