using Back.Application.Interfaces;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using Back.Application.Interfaces.Services;
using Back.Application.UseCases.Certificado;
using Back.Infrastructure.Persistence.Repositories;
using Back.Infrastructure.Services;
using Back.Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Back.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // campus
        services.AddScoped<ICampusRepository, CampusRepository>();

        // curso
        services.AddScoped<ICursoRepository, CursoRepository>();

        // turma
        services.AddScoped<ITurmaRepository, TurmaRepository>();

        //aluno

        services.AddScoped<IAlunoRepository, AlunoRepository>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IEmailTemplateService, EmailTemplateService>();
        services.AddScoped<IConviteCoordenadorRepository, ConviteCoordenadorRepository>();
        services.AddScoped<ICoordenadorRepository, CoordenadorRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAtividadeRepository, AtividadeRepository>();
        services.AddScoped<IAlunoAtividadeRepository, AlunoAtividadeRepository>();
        services.AddScoped<ICertificadoRepository, CertificadoRepository>();
        services.AddScoped<ILimiteHorasAlunoRepository, LimiteHorasAlunoRepository>();
        services.AddScoped<IResetPasswordRepository, ResetPasswordRepository>();
        services.AddScoped<ILembreteEmailRepository, LembreteEmailRepository>();
        services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
        services.AddScoped<ValidarLimiteCertificadoUseCase>();
        services.AddScoped<IIdentityLookupService, IdentityLookupService>();

        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorage"));
        services.AddScoped<IFileStorageService, S3FileStorageService>();

        return services;
    }
}
