using System.Security.Claims;
using Back.Application.DTOs.Certificado;
using Back.Application.UseCases.Certificado;
using Back.Domain.Entities.Certificado;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;

namespace Back.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificadoController : ControllerBase
    {
        private readonly CreateCertificadoUseCase _create;
        private readonly GetCertificadosUseCase _get;
        private readonly AtualizarStatusCertificadoUseCase _atualizar;
        private readonly GetCertificadoByIdUseCase _getById;
        private readonly UpdateCertificadoUseCase _update; 
        private readonly DeleteCertificadoUseCase _delete; 
        public CertificadoController(
            CreateCertificadoUseCase create,
            GetCertificadosUseCase get,
            AtualizarStatusCertificadoUseCase atualizar,
            GetCertificadoByIdUseCase getById,
            UpdateCertificadoUseCase update, 
            DeleteCertificadoUseCase delete)
        {
            _create = create;
            _get = get;
            _atualizar = atualizar;
            _getById = getById;
            _update = update;
            _delete = delete;
        }

        [HttpPost]
        [Authorize(Roles = "ALUNO")]
        [SwaggerOperation(Summary = "Envia um novo certificado para validação.", Tags = new[] { "Certificados" })]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Enviar([FromForm] CreateCertificadoRequest request)
        {
            if (request.Anexo == null || request.Anexo.Length == 0)
                return BadRequest(new { erro = "O anexo é obrigatório e deve conter conteúdo válido." });

            try
            {
                var id = await _create.ExecuteAsync(request);
                return CreatedAtAction(nameof(ObterPorId), new { id }, new { certificadoId = id });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }


        [HttpGet]
        [Authorize(Roles = "COORDENADOR")]
        [SwaggerOperation(Summary = "Lista os certificados com filtros por status e aluno.", Tags = new[] { "Certificados" })]
        [ProducesResponseType(typeof(IEnumerable<CertificadoResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] StatusCertificado? status, [FromQuery] Guid? alunoId)
        {
            var certificados = await _get.ExecuteAsync(status, alunoId);
            return Ok(certificados);
        }

        /// <summary>
        /// Atualiza os dados de um certificado (apenas PENDENTE).
        /// </summary>
        /// <remarks>
        /// Requer permissão de ALUNO.
        /// Permite ao aluno corrigir dados ou trocar o anexo de um certificado
        /// que ainda não foi avaliado (status PENDENTE).
        /// </remarks>
        /// <param name="id">ID do certificado a ser atualizado.</param>
        /// <param name="request">Novos dados do certificado.</param>
        /// <response code="204">Certificado atualizado com sucesso.</response>
        /// <response code="400">Dados inválidos ou certificado não está pendente.</response>
        /// <response code="404">Certificado não encontrado.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "ALUNO")]
        [Consumes("multipart/form-data")]
        [SwaggerOperation(Summary = "Atualiza um certificado PENDENTE.", Tags = new[] { "Certificados" })]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Atualizar(Guid id, [FromForm] UpdateCertificadoRequest request)
        {
            var identityUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(identityUserId))
                return Unauthorized();

            try
            {
                await _update.ExecuteAsync(id, request, identityUserId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { erro = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Remove um certificado do sistema.
        /// </summary>
        /// <remarks>
        /// Requer permissão de COORDENADOR.
        /// Se o certificado estava APROVADO, as horas do aluno serão recalculadas.
        /// </remarks>
        /// <param name="id">ID do certificado a ser removido.</param>
        /// <response code="204">Certificado removido com sucesso.</response>
        /// <response code="404">Certificado não encontrado.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "COORDENADOR")]
        [SwaggerOperation(Summary = "Remove um certificado (ação de Coordenador).", Tags = new[] { "Certificados" })]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
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
            catch (InvalidOperationException ex)
            {
                // Captura erro de "dados corrompidos" do Use Case
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        [SwaggerOperation(Summary = "Obtém detalhes de um certificado pelo ID.", Tags = new[] { "Certificados" })]
        [ProducesResponseType(typeof(CertificadoDetalhadoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ObterPorId(Guid id, [FromServices] GetCertificadoByIdUseCase useCase)
        {
            try
            {
                var certificado = await useCase.ExecuteAsync(id);
                return Ok(certificado);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
        }


        [HttpPatch("{id}/aprovar")]
        [Authorize(Roles = "COORDENADOR")]
        [SwaggerOperation(Summary = "Aprova um certificado enviado pelo aluno.", Tags = new[] { "Certificados" })]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Aprovar(
            Guid id,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)]
            AprovarCertificadoRequest? request = null)
        {
            var ok = await _atualizar.ExecuteAsync(id, StatusCertificado.APROVADO, novaCargaHoraria: request?.NovaCargaHoraria);
            if (!ok)
                return NotFound(new { erro = "Certificado não encontrado." });

            return NoContent();
        }

        [HttpPatch("{id}/reprovar")]
        [Authorize(Roles = "COORDENADOR")]
        [SwaggerOperation(Summary = "Reprova um certificado enviado pelo aluno.", Tags = new[] { "Certificados" })]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Reprovar(Guid id, [FromBody] ReprovarCertificadoRequest request)
        {
            try
            {
                var ok = await _atualizar.ExecuteAsync(id, StatusCertificado.REPROVADO, request.Justificativa);
                if (!ok)
                    return NotFound(new { erro = "Certificado não encontrado." });

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        [HttpGet("me")]
        [Authorize(Roles = "ALUNO")]
        [SwaggerOperation(Summary = "Retorna os certificados do aluno autenticado.", Tags = new[] { "Certificados" })]
        [ProducesResponseType(typeof(IEnumerable<CertificadoResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> MeusCertificados([FromServices] GetCertificadosDoAlunoAutenticadoUseCase useCase)
        {
            var identityUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(identityUserId))
                return Unauthorized();

            try
            {
                var certificados = await useCase.ExecuteAsync(identityUserId);
                return Ok(certificados);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
        }
        [HttpGet("por-curso/{cursoId}")]
        [Authorize(Roles = "COORDENADOR")]
        [SwaggerOperation(Summary = "Lista os certificados por curso ID.", Tags = new[] { "Certificados" })]
        [ProducesResponseType(typeof(IEnumerable<CertificadoPorCursoResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPorCurso(Guid cursoId, [FromServices] GetCertificadosByCursoIdUseCase useCase)
        {
            var result = await useCase.ExecuteAsync(cursoId);
            return Ok(result);
        }
        [HttpGet("{id}/anexo")]
        [Authorize(Roles = "ALUNO,COORDENADOR")]
        [SwaggerOperation(Summary = "Retorna apenas o anexo de um certificado.", Tags = new[] { "Certificados" })]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BaixarAnexo(Guid id, [FromServices] GetCertificadoAnexoUseCase useCase)
        {
            try
            {
                var (content, nomeArquivo, contentType) = await useCase.ExecuteAsync(id);
                return File(content, contentType, nomeArquivo);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
        }

    }
}
