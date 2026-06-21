using Back.Application.Interfaces;
using Back.Application.UseCases.Aluno;
using Back.Application.UseCases.Atividade;
using Back.Application.UseCases.Auth;
using Back.Application.UseCases.Campus;
using Back.Application.UseCases.Certificado;
using Back.Application.UseCases.Coordenador;
using Back.Application.UseCases.Curso;
using Back.Application.UseCases.LembreteEmail;
using Back.Application.UseCases.LimiteHorasAluno;
using Back.Application.UseCases.Turma;
using Microsoft.Extensions.DependencyInjection;

namespace Back.Application;

public static class DependencyInjection
{
    public static object AddApplication(this IServiceCollection services)
    {
        //campus
        services.AddScoped<ListarCampusesUseCase>();
        services.AddScoped<CriarCampusUseCase>();

        //curso
        services.AddScoped<CreateCursoUseCase>();
        services.AddScoped<GetAllCursosUseCase>();
        services.AddScoped<GetCursoByIdUseCase>();
        services.AddScoped<GetResumoCursosUseCase>();
        services.AddScoped<DeleteCursoUseCase>();
        services.AddScoped<UpdateCursoUseCase>();

        //turma
        services.AddScoped<CreateTurmaUseCase>();
        services.AddScoped<GetAllTurmasUseCase>();
        services.AddScoped<GetTurmaByIdUseCase>();
        services.AddScoped<VerificarTurmaExisteUseCase>();
        services.AddScoped<GetAlunosByTurmaUseCase>();
        services.AddScoped<GetTurmasByCursoIdUseCase>();
        services.AddScoped<DeleteTurmaUseCase>();
        services.AddScoped<UpdateTurmaUseCase>();
        services.AddScoped<ToggleCodigoUseCase>();
        services.AddScoped<ResetarCodigoUseCase>();
        services.AddScoped<GetPeriodosLetivosUseCase>();

        //Aluno
        services.AddScoped<CreateAlunoUseCase>();
        services.AddScoped<GetAlunoByIdUseCase>();
        services.AddScoped<DeleteAlunoUseCase>();
        services.AddScoped<ToggleAlunoStatusUseCase>();
        services.AddScoped<GetAlunoDetalhadoUseCase>();
        services.AddScoped<GetResumoHorasUseCase>();
        services.AddScoped<GetAlunoFromTokenUseCase>();
        services.AddScoped<GetAlunosComHorasConcluidasUseCase>();
        services.AddScoped<ContarPendenciasDownloadUseCase>();
        services.AddScoped<MarcarDownloadRelatorioUseCase>();
        services.AddScoped<DeleteAlunoUseCase>();
        services.AddScoped<UpdateAlunoUseCase>();
        services.AddScoped<GetAlunosEmRiscoUseCase>();
        //auth
        services.AddScoped<LoginUseCase>();
        services.AddScoped<ForgotPasswordUseCase>();
        services.AddScoped<ValidateResetCodeUseCase>();
        services.AddScoped<ResetPasswordUseCase>();

        //coordenador
        services.AddScoped<EnviarConviteUseCase>();
        services.AddScoped<CriarCoordenadorUseCase>();
        services.AddScoped<GetAllAtividadesUseCase>();
        services.AddScoped<GetCoordenadorFromTokenUseCase>();
        services.AddScoped<GetCoordenadorByCursoIdUseCase>();
        services.AddScoped<DeleteCoordenadorUseCase>();
        services.AddScoped<UpdateCoordenadorAdminUseCase>();
        services.AddScoped<UpdateCoordenadorSelfUseCase>();
        //certificado
        services.AddScoped<CreateCertificadoUseCase>();
        services.AddScoped<GetCertificadosUseCase>();
        services.AddScoped<GetCertificadosDoAlunoAutenticadoUseCase>();
        services.AddScoped<AtualizarStatusCertificadoUseCase>();
        services.AddScoped<INotificarSecretariaConclusaoUseCase, NotificarSecretariaConclusaoUseCase>();
        services.AddScoped<GetCertificadoByIdUseCase>();
        services.AddScoped<GetCertificadosByCursoIdUseCase>();
        services.AddScoped<GetCertificadoAnexoUseCase>();
        services.AddScoped<UpdateCertificadoUseCase>();
        services.AddScoped<DeleteCertificadoUseCase>();
        services.AddScoped<GetPeriodosLetivosValidosDoAlunoUseCase>();
        //lembrete de e-mail
        services.AddScoped<ResolverCursoCoordenadorUseCase>();
        services.AddScoped<CriarLembreteEmailUseCase>();
        services.AddScoped<ListarLembretesEmailUseCase>();
        services.AddScoped<AtualizarLembreteEmailUseCase>();
        services.AddScoped<RemoverLembreteEmailUseCase>();
        services.AddScoped<EnviarLembretesPendentesUseCase>();
        //Limitehoras
        services.AddScoped<CreateLimiteHorasAlunoUseCase>();
        //atividade
        services.AddScoped<CreateAtividadeUseCase>();
        services.AddScoped<DeleteAtividadeUseCase>();
        services.AddScoped<UpdateAtividadeUseCase>();
        return services;
    }
}
