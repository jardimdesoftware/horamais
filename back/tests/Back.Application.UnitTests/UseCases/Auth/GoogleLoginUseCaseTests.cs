using Back.Application.DTOs.Auth;
using Back.Application.Interfaces.Identity;
using Back.Application.UseCases.Auth;
using FluentAssertions;
using Moq;

namespace Back.Application.UnitTests.UseCases.Auth;

public class GoogleLoginUseCaseTests
{
    private readonly Mock<IAuthService> _auth = new();

    private GoogleLoginUseCase CreateUseCase()
        => new GoogleLoginUseCase(_auth.Object);

    [Fact]
    public async Task Deve_Retornar_LoginResponse()
    {
        var response = new GoogleLoginResponseDto("User", "a@b.com", "ALUNO", "token123");

        _auth.Setup(a => a.LoginWithGoogleAsync(It.IsAny<GoogleLoginRequestDto>()))
            .ReturnsAsync(response);

        var useCase = CreateUseCase();

        var result = await useCase.ExecuteAsync(new GoogleLoginRequestDto("id-token-fake"));

        result.Should().Be(response);
    }

    [Theory]
    [InlineData("aluno@discente.ifpe.edu.br")]
    [InlineData("aluno@DISCENTE.IFPE.EDU.BR")]
    public void PrimeiroAcesso_Discente_NaoEmiteTokenOuPerfil(string email)
    {
        var result = GoogleLoginResponseDto.ForFirstAccess("Aluno", email);
        result.RequiresRegistration.Should().BeTrue();
        result.Email.Should().Be(email);
        result.Token.Should().BeNull();
        result.Role.Should().BeNull();
    }

    [Theory]
    [InlineData("aluno@gmail.com")]
    [InlineData("aluno@ifpe.edu.br")]
    [InlineData("aluno@sub.discente.ifpe.edu.br")]
    [InlineData("aluno@discente.ifpe.edu.br.evil.com")]
    [InlineData("aluno@evil.com@discente.ifpe.edu.br")]
    [InlineData("@discente.ifpe.edu.br")]
    public void PrimeiroAcesso_OutrosDominios_Recusa(string email)
    {
        var act = () => GoogleLoginResponseDto.ForFirstAccess("Aluno", email);
        act.Should().Throw<UnauthorizedAccessException>();
    }
}
