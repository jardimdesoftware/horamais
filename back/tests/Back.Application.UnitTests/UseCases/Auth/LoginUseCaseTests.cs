using Back.Application.DTOs.Auth;
using Back.Application.Interfaces.Identity;
using Back.Application.UseCases.Auth;
using FluentAssertions;
using Moq;

namespace Back.Application.UnitTests.UseCases.Auth;

public class LoginUseCaseTests
{
    private readonly Mock<IAuthService> _auth = new();

    private LoginUseCase CreateUseCase()
        => new LoginUseCase(_auth.Object);

    [Fact]
    public async Task Deve_Retornar_LoginResponse()
    {
        var response = new LoginResponseDto("User", "a@b.com", "ALUNO", "token123");

        _auth.Setup(a => a.LoginAsync(It.IsAny<LoginRequestDto>()))
            .ReturnsAsync(response);

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(new LoginRequestDto("a@b.com", "123"));

        result.Should().Be(response);
    }
}
