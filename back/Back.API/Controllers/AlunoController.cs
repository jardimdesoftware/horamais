using Back.Application.DTOs.Aluno;
using Back.Application.UseCases.Aluno;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Back.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlunoController : ControllerBase
{
    private readonly CreateAlunoUseCase _create;
    private readonly GetAlunoByIdUseCase _getById;
    private readonly DeleteAlunoUseCase _delete;
    private readonly ToggleAlunoStatusUseCase _toggle;
    private readonly GetAlunoDetalhadoUseCase _getDetalhado;
    private readonly GetResumoHorasUseCase _getResumo;
    private readonly GetAlunoFromTokenUseCase _getMeFromToken;
    private readonly GetAlunosComHorasConcluidasUseCase _getAlunosComHorasConcluidas;
    private readonly ContarPendenciasDownloadUseCase _contarPendenciasDownload;
    private readonly MarcarDownloadRelatorioUseCase _marcarDownloadRelatorio;
    private readonly UpdateAlunoUseCase _update;
    private readonly GetAlunosEmRiscoUseCase _getEmRisco;
    public AlunoController(
        CreateAlunoUseCase create,
        GetAlunoByIdUseCase getById,
        DeleteAlunoUseCase delete,
        ToggleAlunoStatusUseCase toggle,
        GetAlunoDetalhadoUseCase getDetalhado,
        GetResumoHorasUseCase getResumo,
        GetAlunoFromTokenUseCase getMeFromToken,
        GetAlunosComHorasConcluidasUseCase getAlunosComHorasConcluidas,
        ContarPendenciasDownloadUseCase contarPendenciasDownload,
        MarcarDownloadRelatorioUseCase marcarDownloadRelatorio,
        UpdateAlunoUseCase update,
        GetAlunosEmRiscoUseCase getEmRisco)
    {
        _create = create;
        _getById = getById;
        _delete = delete;
        _toggle = toggle;
        _getDetalhado = getDetalhado;
        _getResumo = getResumo;
        _getMeFromToken = getMeFromToken;
        _getAlunosComHorasConcluidas = getAlunosComHorasConcluidas;
        _contarPendenciasDownload = contarPendenciasDownload;
        _marcarDownloadRelatorio = marcarDownloadRelatorio;
        _update = update;
        _getEmRisco = getEmRisco;
    }

    /// <summary>
    /// Cadastra um novo aluno.
    /// </summary>
    /// <remarks>Requer permissão de COORDENADOR.</remarks>
    /// <param name="request">Dados do aluno a ser criado.</param>
    /// <response code="201">Aluno criado com sucesso.</response>
    /// <response code="400">Dados inválidos ou erro de validação.</response>
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Criar([FromBody] CreateAlunoRequest request)
    {
        var result = await _create.ExecuteAsync(request);
        return CreatedAtAction(nameof(ObterPorId), new { id = result.Id }, result);
    }

    /// <summary>
    /// Obtém os dados de um aluno pelo ID.
    /// </summary>
    /// <remarks>Requer permissão de COORDENADOR.</remarks>
    /// <param name="id">ID do aluno.</param>
    /// <response code="200">Dados do aluno retornados com sucesso.</response>
    /// <response code="404">Aluno não encontrado.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = "COORDENADOR")]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var aluno = await _getById.ExecuteAsync(id);
        return Ok(aluno);
    }

    /// <summary>
    /// Atualiza os dados de um aluno.
    /// </summary>
    /// <remarks>Requer permissão de COORDENADOR.</remarks>
    /// <param name="id">ID do aluno a ser atualizado.</param>
    /// <param name="request">Novos dados do aluno.</param>
    /// <response code="200">Aluno atualizado com sucesso.</response>
    /// <response code="400">Dados inválidos.</response>
    /// <response code="404">Aluno ou Turma não encontrados.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "COORDENADOR")]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] UpdateAlunoRequest request)
    {
        var result = await _update.ExecuteAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Remove um aluno do sistema (remoção permanente).
    /// </summary>
    /// <remarks>Requer permissão de COORDENADOR.</remarks>
    /// <param name="id">ID do aluno a ser excluído.</param>
    /// <response code="204">Aluno removido com sucesso.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "COORDENADOR")]
    public async Task<IActionResult> Deletar(Guid id)
    {
        await _delete.ExecuteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Ativa ou desativa o status de um aluno.
    /// </summary>
    /// <remarks>Requer permissão de COORDENADOR.</remarks>
    /// <param name="id">ID do aluno.</param>
    /// <response code="204">Status alterado com sucesso.</response>
    [HttpPatch("{id:guid}/toggle-status")]
    [Authorize(Roles = "COORDENADOR")]
    public async Task<IActionResult> AtivarDesativar(Guid id)
    {
        await _toggle.ExecuteAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Obtém os dados detalhados de um aluno, incluindo atividades e horas concluídas.
    /// </summary>
    /// <remarks>Requer permissão de COORDENADOR.</remarks>
    /// <param name="id">ID do aluno.</param>
    /// <response code="200">Dados detalhados do aluno retornados com sucesso.</response>
    [HttpGet("{id:guid}/detalhado")]
    [Authorize(Roles = "COORDENADOR")]
    public async Task<IActionResult> ObterDetalhado(Guid id)
    {
        var aluno = await _getDetalhado.ExecuteAsync(id);
        return Ok(aluno);
    }

    /// <summary>
    /// Lista todos os alunos com resumo das horas concluídas em atividades de extensão e complementar.
    /// </summary>
    /// <remarks>Requer permissão de COORDENADOR.</remarks>
    /// <response code="200">Resumo de horas retornado com sucesso.</response>
    [HttpGet("resumo-horas")]
    [Authorize(Roles = "COORDENADOR")]
    public async Task<IActionResult> ListarResumo()
    {
        var result = await _getResumo.ExecuteAsync();
        return Ok(result);
    }

    /// <summary>
    /// Obtém os dados detalhados do aluno autenticado.
    /// </summary>
    /// <remarks>Requer permissão de ALUNO autenticado.</remarks>
    /// <response code="200">Dados do aluno retornados com sucesso.</response>
    /// <response code="401">Usuário não autenticado.</response>
    [HttpGet("meu-detalhado")]
    [Authorize(Roles = "ALUNO")]
    public async Task<IActionResult> ObterMeusDados()
    {
        var result = await _getMeFromToken.ExecuteAsync(User);
        return Ok(result);
    }
    /// <summary>
    /// Lista os alunos do curso do coordenador que concluíram 100% das horas complementares.
    /// </summary>
    /// <remarks>Requer permissão de COORDENADOR. A busca é filtrada pelo curso do coordenador autenticado.</remarks>
    /// <response code="200">Alunos retornados com sucesso.</response>
    /// <response code="401">Coordenador não autenticado ou não encontrado.</response>
    [HttpGet("concluidos/complementar")]
    [Authorize(Roles = "COORDENADOR")]
    public async Task<IActionResult> ObterConcluidosComplementar()
    {
        // Passando o "User" (ClaimsPrincipal) para o UseCase
        var result = await _getAlunosComHorasConcluidas.ExecuteAsync(Domain.Entities.Atividade.TipoAtividade.COMPLEMENTAR, User);
        return Ok(result);
    }

    /// <summary>
    /// Lista os alunos do curso do coordenador que concluíram 100% das horas de extensão.
    /// </summary>
    /// <remarks>Requer permissão de COORDENADOR. A busca é filtrada pelo curso do coordenador autenticado.</remarks>
    /// <response code="200">Alunos retornados com sucesso.</response>
    /// <response code="401">Coordenador não autenticado ou não encontrado.</response>
    [HttpGet("concluidos/extensao")]
    [Authorize(Roles = "COORDENADOR")]
    public async Task<IActionResult> ObterConcluidosExtensao()
    {
        // Passando o "User" (ClaimsPrincipal) para o UseCase
        var result = await _getAlunosComHorasConcluidas.ExecuteAsync(Domain.Entities.Atividade.TipoAtividade.EXTENSAO, User);
        return Ok(result);
    }
    /// <summary>
    /// Conta o total de pendências de download de relatórios para alunos que concluíram as horas.
    /// </summary>
    /// <remarks>
    /// Requer permissão de COORDENADOR.
    /// Conta +1 para cada tipo de hora (Complementar/Extensão) que um aluno concluiu mas ainda não fez o download.
    /// Um aluno pode contribuir com 0, 1 ou 2 para o total.
    /// </remarks>
    /// <response code="200">Contagem retornada com sucesso.</response>
    /// <response code="401">Coordenador não autenticado ou não encontrado.</response>
    [HttpGet("pendencias-download/contagem")]
    [Authorize(Roles = "COORDENADOR")]
    public async Task<IActionResult> ContarPendenciasDownload()
    {
        var result = await _contarPendenciasDownload.ExecuteAsync(User);
        return Ok(result);
    }
    /// <summary>
    /// Marca o status do relatório de horas complementares de um aluno como "baixado".
    /// </summary>
    /// <remarks>Requer permissão de COORDENADOR. Só pode ser executado em alunos do próprio curso.</remarks>
    /// <param name="alunoId">ID do aluno a ser modificado.</param>
    /// <response code="204">Status alterado com sucesso.</response>
    /// <response code="401">Coordenador não autorizado.</response>
    /// <response code="404">Aluno não encontrado.</response>
    [HttpPatch("{alunoId:guid}/marcar-download/complementar")]
    [Authorize(Roles = "COORDENADOR")]
    public async Task<IActionResult> MarcarDownloadComplementar(Guid alunoId)
    {
        await _marcarDownloadRelatorio.ExecuteAsync(alunoId, Domain.Entities.Atividade.TipoAtividade.COMPLEMENTAR, User);
        return NoContent(); // HTTP 204: Sucesso, sem conteúdo para retornar.
    }

    /// <summary>
    /// Marca o status do relatório de horas de extensão de um aluno como "baixado".
    /// </summary>
    /// <remarks>Requer permissão de COORDENADOR. Só pode ser executado em alunos do próprio curso.</remarks>
    /// <param name="alunoId">ID do aluno a ser modificado.</param>
    /// <response code="204">Status alterado com sucesso.</response>
    /// <response code="401">Coordenador não autorizado.</response>
    /// <response code="404">Aluno não encontrado.</response>
    [HttpPatch("{alunoId:guid}/marcar-download/extensao")]
    [Authorize(Roles = "COORDENADOR")]
    public async Task<IActionResult> MarcarDownloadExtensao(Guid alunoId)
    {
        await _marcarDownloadRelatorio.ExecuteAsync(alunoId, Domain.Entities.Atividade.TipoAtividade.EXTENSAO, User);
        return NoContent();
    }

    /// <summary>
    /// Lista alunos ativos com progresso abaixo do percentual informado, ordenados por horas/período (menor primeiro).
    /// </summary>
    /// <remarks>
    /// A métrica <c>horasPorPeriodo</c> é calculada dividindo o total de horas complementares pelo número
    /// de períodos letivos decorridos desde o início da turma (formato "YYYY.S").
    /// Requer permissão de COORDENADOR.
    /// </remarks>
    /// <param name="percentualMaximo">Percentual máximo de conclusão (0–100). Padrão: 50.</param>
    /// <response code="200">Lista de alunos em risco retornada com sucesso.</response>
    [HttpGet("risco")]
    [Authorize(Roles = "COORDENADOR")]
    public async Task<IActionResult> ListarEmRisco([FromQuery] double percentualMaximo = 50)
    {
        if (percentualMaximo < 0 || percentualMaximo > 100)
            return BadRequest("percentualMaximo deve estar entre 0 e 100.");

        var result = await _getEmRisco.ExecuteAsync(percentualMaximo);
        return Ok(result);
    }
}
