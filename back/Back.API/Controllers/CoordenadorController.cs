using Back.Application.DTOs.Coordenador;
using Back.Application.UseCases.Coordenador;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Back.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoordenadorController : ControllerBase
{
    private readonly EnviarConviteUseCase _enviarConvite;
    private readonly CriarCoordenadorUseCase _criarCoordenador;
    private readonly GetCoordenadorFromTokenUseCase _getFromToken;
    private readonly GetCoordenadorByCursoIdUseCase _getByCursoId;
    private readonly UpdateCoordenadorSelfUseCase _updateSelf;
    private readonly UpdateCoordenadorAdminUseCase _updateAdmin;
    private readonly DeleteCoordenadorUseCase _delete;

    public CoordenadorController(
        EnviarConviteUseCase enviarConvite,
        CriarCoordenadorUseCase criarCoordenador,
        GetCoordenadorFromTokenUseCase getFromToken,
        GetCoordenadorByCursoIdUseCase getByCursoId,
        UpdateCoordenadorSelfUseCase updateSelf,    
        UpdateCoordenadorAdminUseCase updateAdmin, 
        DeleteCoordenadorUseCase delete)           
    {
        _enviarConvite = enviarConvite;
        _criarCoordenador = criarCoordenador;
        _getFromToken = getFromToken;
        _getByCursoId = getByCursoId;
        _updateSelf = updateSelf;     
        _updateAdmin = updateAdmin;  
        _delete = delete;    
    }

    /// <summary>
    /// Envia um convite para um novo coordenador (somente ADMIN).
    /// </summary>
    /// <remarks>
    /// Um email de convite é enviado para o endereço informado. Apenas usuários com perfil ADMIN podem executar esta ação.
    /// </remarks>
    /// <param name="request">Dados do convite</param>
    /// <returns>Mensagem de confirmação</returns>
    [HttpPost("convite")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(
        Summary = "Envia um convite para um novo coordenador.",
        Description = "Somente usuários com o papel ADMIN podem enviar convites para coordenadores.",
        Tags = new[] { "Coordenadores" })]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> EnviarConvite([FromBody] ConviteCoordenadorRequest request)
    {
        await _enviarConvite.ExecuteAsync(request);
        return Ok(new { mensagem = "Convite enviado com sucesso." });
    }

    /// <summary>
    /// Cria a conta do coordenador a partir de um convite com token válido.
    /// </summary>
    /// <remarks>
    /// Esta rota é usada para que o próprio coordenador conclua o cadastro após receber o convite.
    /// </remarks>
    /// <param name="request">Dados de cadastro + token do convite</param>
    /// <returns>Dados da conta criada</returns>
    [HttpPost("cadastrar")]
    [AllowAnonymous]
    [SwaggerOperation(
        Summary = "Cadastra um coordenador a partir de um convite.",
        Description = "Não requer autenticação. O token do convite é obrigatório.",
        Tags = new[] { "Coordenadores" })]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Cadastrar([FromBody] CadastroCoordenadorRequest request)
    {
        var result = await _criarCoordenador.ExecuteAsync(request);
        return Ok(result);
    }
    [HttpGet("me")]
    [Authorize(Roles = "COORDENADOR")]
    [SwaggerOperation(Summary = "Retorna os dados do coordenador autenticado.", Tags = new[] { "Coordenadores" })]
    [ProducesResponseType(typeof(CoordenadorInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Me()
    {
        var info = await _getFromToken.ExecuteAsync(User);
        return Ok(info);
    }


    /// <summary>
    /// Atualiza os dados do próprio coordenador autenticado.
    /// </summary>
    /// <remarks>
    /// Requer permissão de COORDENADOR. Não permite alterar o curso.
    /// </remarks>
    [HttpPut("me")]
    [Authorize(Roles = "COORDENADOR")]
    [SwaggerOperation(Summary = "Atualiza os dados do próprio coordenador.", Tags = new[] { "Coordenadores" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AtualizarMeusDados([FromBody] UpdateCoordenadorSelfRequest request)
    {
        await _updateSelf.ExecuteAsync(User, request);
        return NoContent();
    }

    /// <summary>
    /// (ADMIN) Atualiza os dados de um coordenador específico.
    /// </summary>
    /// <remarks>
    /// Requer permissão de ADMIN. Permite alterar o curso do coordenador.
    /// </remarks>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(Summary = "(ADMIN) Atualiza dados de um coordenador.", Tags = new[] { "Coordenadores" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> AtualizarCoordenador(Guid id, [FromBody] UpdateCoordenadorAdminRequest request)
    {
        await _updateAdmin.ExecuteAsync(id, request);
        return NoContent();
    }

    /// <summary>
    /// (ADMIN) Remove um coordenador do sistema.
    /// </summary>
    /// <remarks>
    /// Requer permissão de ADMIN. Remove o coordenador e sua conta de usuário.
    /// </remarks>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(Summary = "(ADMIN) Remove um coordenador.", Tags = new[] { "Coordenadores" })]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeletarCoordenador(Guid id)
    {
        await _delete.ExecuteAsync(id);
        return NoContent();
    }
    [HttpGet("por-curso/{cursoId:guid}")]
    [Authorize(Roles = "ADMIN")]
    [SwaggerOperation(Summary = "Retorna o coordenador de um curso.", Tags = new[] { "Coordenadores" })]
    [ProducesResponseType(typeof(CoordenadorResumoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorCurso(Guid cursoId)
    {
        var coordenador = await _getByCursoId.ExecuteAsync(cursoId);

        if (coordenador == null)
            return NotFound(new { mensagem = "Nenhum coordenador encontrado para este curso." });

        return Ok(coordenador);
    }
}
