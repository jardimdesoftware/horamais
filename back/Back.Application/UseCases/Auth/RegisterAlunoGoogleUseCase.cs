using Back.Application.DTOs.Aluno;
using Back.Application.Interfaces.Identity;
using Back.Application.UseCases.Aluno;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Auth;

public class RegisterAlunoGoogleUseCase
{
    private readonly IAuthService _authService;
    private readonly CreateAlunoUseCase _createAluno;

    public RegisterAlunoGoogleUseCase(IAuthService authService, CreateAlunoUseCase createAluno)
    {
        _authService = authService;
        _createAluno = createAluno;
    }

    public async Task<CreateAlunoResponse> ExecuteAsync(CreateAlunoGoogleRequest request)
    {
        var verifiedEmail = _authService.ValidateGoogleRegistrationTicket(request.RegistrationTicket);
        return await _createAluno.ExecuteWithGoogleAsync(request, verifiedEmail);
    }
}
