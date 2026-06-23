// Back.API/Controllers/AuthController.cs (adicionando endpoints)
using Back.Application.DTOs.Auth;
using Back.Application.UseCases.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Threading.Tasks;

namespace Back.API.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly LoginUseCase _loginUseCase;
    private readonly ForgotPasswordUseCase _forgotPasswordUseCase;
    private readonly ValidateResetCodeUseCase _validateResetCodeUseCase;
    private readonly ResetPasswordUseCase _resetPasswordUseCase;
    private readonly ConfirmarEmailUseCase _confirmarEmailUseCase;
    private readonly ReenviarVerificacaoUseCase _reenviarVerificacaoUseCase;

    public AuthController(
        LoginUseCase loginUseCase,
        ForgotPasswordUseCase forgotPasswordUseCase,
        ValidateResetCodeUseCase validateResetCodeUseCase,
        ResetPasswordUseCase resetPasswordUseCase,
        ConfirmarEmailUseCase confirmarEmailUseCase,
        ReenviarVerificacaoUseCase reenviarVerificacaoUseCase)
    {
        _loginUseCase = loginUseCase;
        _forgotPasswordUseCase = forgotPasswordUseCase;
        _validateResetCodeUseCase = validateResetCodeUseCase;
        _resetPasswordUseCase = resetPasswordUseCase;
        _confirmarEmailUseCase = confirmarEmailUseCase;
        _reenviarVerificacaoUseCase = reenviarVerificacaoUseCase;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(400)]
    [SwaggerOperation(Summary = "Autenticar usuário", Description = "Realiza o login de um usuário com base nas credenciais informadas.")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        try
        {
            var result = await _loginUseCase.ExecuteAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ForgotPasswordResponseDto), 200)]
    [SwaggerOperation(Summary = "Solicitar código de redefinição", Description = "Gera um código de 6 dígitos e envia por e-mail.")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
    {
        var result = await _forgotPasswordUseCase.ExecuteAsync(dto);
        return Ok(result);
    }

    [HttpPost("validate-code")]
    [ProducesResponseType(typeof(ValidateCodeResponseDto), 200)]
    [SwaggerOperation(Summary = "Validar código", Description = "Valida o código de 6 dígitos enviado por e-mail.")]
    public async Task<IActionResult> ValidateCode([FromBody] ValidateCodeRequestDto dto)
    {
        var result = await _validateResetCodeUseCase.ExecuteAsync(dto);
        return Ok(result);
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ResetPasswordResponseDto), 200)]
    [ProducesResponseType(400)]
    [SwaggerOperation(Summary = "Redefinir senha", Description = "Redefine a senha usando o token do Identity associado ao código validado.")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto dto)
    {
        try
        {
            var result = await _resetPasswordUseCase.ExecuteAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("confirm-email")]
    [ProducesResponseType(typeof(ConfirmEmailResponseDto), 200)]
    [ProducesResponseType(400)]
    [SwaggerOperation(Summary = "Confirmar e-mail", Description = "Confirma o e-mail de um cadastro pendente usando o código de 6 dígitos enviado por e-mail.")]
    public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailRequestDto dto)
    {
        try
        {
            var result = await _confirmarEmailUseCase.ExecuteAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("resend-verification")]
    [ProducesResponseType(typeof(ResendVerificationResponseDto), 200)]
    [SwaggerOperation(Summary = "Reenviar verificação", Description = "Reenvia o código de verificação de e-mail para um cadastro ainda pendente.")]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequestDto dto)
    {
        var result = await _reenviarVerificacaoUseCase.ExecuteAsync(dto);
        return Ok(result);
    }
}
