using Back.Application.DTOs.Auth;
using Back.Application.Interfaces.Identity;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Auth;

public class GoogleLoginUseCase
{
    private readonly IAuthService _authService;

    public GoogleLoginUseCase(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<GoogleLoginResponseDto> ExecuteAsync(GoogleLoginRequestDto dto)
    {
        return await _authService.LoginWithGoogleAsync(dto);
    }
}
