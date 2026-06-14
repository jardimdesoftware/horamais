using Back.Application.DTOs.LembreteEmail;
using Back.Application.UseCases.LembreteEmail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Back.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "COORDENADOR")]
public class LembreteEmailController : ControllerBase
{
    private readonly ResolverCursoCoordenadorUseCase _resolverCurso;
    private readonly ListarLembretesEmailUseCase _listar;
    private readonly CriarLembreteEmailUseCase _criar;
    private readonly AtualizarLembreteEmailUseCase _atualizar;
    private readonly RemoverLembreteEmailUseCase _remover;

    public LembreteEmailController(
        ResolverCursoCoordenadorUseCase resolverCurso,
        ListarLembretesEmailUseCase listar,
        CriarLembreteEmailUseCase criar,
        AtualizarLembreteEmailUseCase atualizar,
        RemoverLembreteEmailUseCase remover)
    {
        _resolverCurso = resolverCurso;
        _listar = listar;
        _criar = criar;
        _atualizar = atualizar;
        _remover = remover;
    }

    /// <summary>
    /// Lista as datas de lembrete cadastradas para o curso do coordenador autenticado.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Lista as datas de lembrete do curso.", Tags = new[] { "Lembretes" })]
    [ProducesResponseType(typeof(IEnumerable<LembreteEmailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar()
    {
        var cursoId = await _resolverCurso.ExecuteAsync(User);
        var lembretes = await _listar.ExecuteAsync(cursoId);
        return Ok(lembretes);
    }

    /// <summary>
    /// Cadastra uma nova data de lembrete (máximo de 3 por curso).
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Cadastra uma data de lembrete.", Tags = new[] { "Lembretes" })]
    [ProducesResponseType(typeof(LembreteEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] LembreteEmailRequest request)
    {
        var cursoId = await _resolverCurso.ExecuteAsync(User);
        var lembrete = await _criar.ExecuteAsync(cursoId, request);
        return Ok(lembrete);
    }

    /// <summary>
    /// Atualiza a data ou a mensagem de um lembrete ainda não enviado.
    /// </summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Atualiza uma data de lembrete.", Tags = new[] { "Lembretes" })]
    [ProducesResponseType(typeof(LembreteEmailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] LembreteEmailRequest request)
    {
        var cursoId = await _resolverCurso.ExecuteAsync(User);
        var lembrete = await _atualizar.ExecuteAsync(cursoId, id, request);
        return Ok(lembrete);
    }

    /// <summary>
    /// Remove uma data de lembrete do curso.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Remove uma data de lembrete.", Tags = new[] { "Lembretes" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id)
    {
        var cursoId = await _resolverCurso.ExecuteAsync(User);
        await _remover.ExecuteAsync(cursoId, id);
        return NoContent();
    }
}
