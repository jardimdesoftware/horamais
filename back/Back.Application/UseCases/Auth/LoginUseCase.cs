using Back.Application.DTOs.Auth;
using Back.Application.Interfaces.Identity;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Auth;

public class LoginUseCase
{
    private readonly IAuthService _authService;

    public LoginUseCase(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<LoginResponseDto> ExecuteAsync(LoginRequestDto dto)
    {
        return await _authService.LoginAsync(dto);
    }
}
