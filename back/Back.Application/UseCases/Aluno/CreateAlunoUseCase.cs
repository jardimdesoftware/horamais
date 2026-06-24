using Back.Application.DTOs.Aluno;
using Back.Application.Interfaces;
using Back.Application.Interfaces.Identity;
using Back.Application.Interfaces.Repositories;
using Back.Application.Interfaces.Services;
using Back.Domain.Entities.Aluno;
using Back.Domain.Entities.AlunoAtividade;
using Back.Domain.Entities.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Aluno;

public class CreateAlunoUseCase
{
    private const string AssuntoVerificacao = "Confirme seu e-mail — hora+";

    private readonly IAlunoRepository _alunoRepo;
    private readonly ITurmaRepository _turmaRepo;
    private readonly IAtividadeRepository _atividadeRepo;
    private readonly IAlunoAtividadeRepository _alunoAtividadeRepo;
    private readonly IIdentityService _identityService;
    private readonly ITurmaRealtimeNotifier _realtime;
    private readonly IEmailVerificationRepository _verificationRepo;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;

    public CreateAlunoUseCase(
        IAlunoRepository alunoRepo,
        ITurmaRepository turmaRepo,
        IAtividadeRepository atividadeRepo,
        IAlunoAtividadeRepository alunoAtividadeRepo,
        IIdentityService identityService,
        ITurmaRealtimeNotifier realtime,
        IEmailVerificationRepository verificationRepo,
        IEmailService emailService,
        IEmailTemplateService templateService)
    {
        _alunoRepo = alunoRepo;
        _turmaRepo = turmaRepo;
        _atividadeRepo = atividadeRepo;
        _alunoAtividadeRepo = alunoAtividadeRepo;
        _identityService = identityService;
        _realtime = realtime;
        _verificationRepo = verificationRepo;
        _emailService = emailService;
        _templateService = templateService;
    }

    public async Task<CreateAlunoResponse> ExecuteAsync(CreateAlunoRequest request)
    {
        if (!request.Email.EndsWith("@ifpe.edu.br", StringComparison.OrdinalIgnoreCase) &&
            !request.Email.EndsWith(".ifpe.edu.br", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Email institucional inválido.");

        // Busca a turma por ID ou Código
        Back.Domain.Entities.Turma.Turma? turma = null;
        if (request.TurmaId.HasValue)
        {
            turma = await _turmaRepo.GetByIdAsync(request.TurmaId.Value);
        }
        else if (!string.IsNullOrEmpty(request.TurmaCodigo))
        {
            turma = await _turmaRepo.GetByCodigoAsync(request.TurmaCodigo);
        }

        if (turma == null)
            throw new InvalidOperationException("Turma não encontrada.");

        // Criação do usuário no Identity com e-mail NÃO confirmado: a conta fica
        // pendente até o aluno confirmar o código enviado por e-mail (gating no login).
        var (success, userId, errors) = await _identityService.CreateUserAsync(
            request.Email, request.Senha, "ALUNO", emailConfirmed: false);

        if (!success)
            throw new InvalidOperationException("Erro ao criar usuário: " + string.Join("; ", errors));

        var alunoId = Guid.NewGuid();

        // Criação do aluno com IsAtivo = true (via builder)
        var aluno = new AlunoBuilder()
            .WithId(alunoId)
            .WithNome(request.Nome)
            .WithEmail(request.Email)
            .WithMatricula(request.Matricula)
            .WithTurmaId(turma.Id)
            .WithIdentityUserId(userId)
            .Build();

        await _alunoRepo.AddAsync(aluno);

        // Atividades são globais: vincula o aluno a TODAS as atividades
        var atividades = await _atividadeRepo.GetAllAsync();

        var alunoAtividades = atividades.Select(atividade =>
            new AlunoAtividadeBuilder()
                .WithId(Guid.NewGuid())
                .WithAlunoId(alunoId)
                .WithAtividadeId(atividade.Id)
                .WithHorasConcluidas(0)
                .Build()
        ).ToList();

        await _alunoAtividadeRepo.AddRangeAsync(alunoAtividades);

        // Gera e envia o código de verificação de e-mail (validade de 24h).
        var codigo = GerarCodigoSeisDigitos();
        await _verificationRepo.AddAsync(new EmailVerificationCode
        {
            Id = Guid.NewGuid(),
            IdentityUserId = userId,
            Code = codigo,
            ExpiresAtUtc = DateTime.UtcNow.AddHours(24)
        });
        await _verificationRepo.SaveChangesAsync();

        var corpo = _templateService.RenderVerificacaoEmail(aluno.Nome ?? "aluno", codigo);
        await _emailService.EnviarEmailAsync(aluno.Email!, AssuntoVerificacao, corpo);

        // Avisa os coordenadores com esta turma aberta para que a lista atualize
        // em tempo real, sem recarregar a página.
        await _realtime.NotificarAlunosAlteradosAsync(turma.Id);

        return new CreateAlunoResponse(aluno.Id, aluno.Nome!, aluno.Email!);
    }

    private static string GerarCodigoSeisDigitos()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var value = BitConverter.ToUInt32(bytes, 0) % 1_000_000;
        return value.ToString("D6");
    }
}
