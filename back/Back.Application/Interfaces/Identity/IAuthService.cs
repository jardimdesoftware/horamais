using Back.Application.DTOs.Auth;
using System.Threading.Tasks;

namespace Back.Application.Interfaces.Identity;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);
}
