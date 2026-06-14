using Back.Application.DTOs.Coordenador;
using Back.Application.Interfaces;
using Back.Application.Interfaces.Repositories;
using Back.Domain.Entities.Convite;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Back.Application.UseCases.Coordenador;

public class EnviarConviteUseCase
{
    private readonly IConviteCoordenadorRepository _conviteRepo;
    private readonly IEmailService _emailService;
    private readonly IEmailTemplateService _templateService;
    private readonly IConfiguration _config;

    public EnviarConviteUseCase(
        IConviteCoordenadorRepository conviteRepo,
        IEmailService emailService,
        IEmailTemplateService templateService,
        IConfiguration config)
    {
        _conviteRepo = conviteRepo;
        _emailService = emailService;
        _templateService = templateService;
        _config = config;
    }

    public async Task ExecuteAsync(ConviteCoordenadorRequest request)
    {
        if (!request.Email.EndsWith("@ifpe.edu.br", StringComparison.OrdinalIgnoreCase) &&
            !request.Email.EndsWith(".ifpe.edu.br", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Email institucional inválido. Use um endereço institucional (ifpe.edu.br).");

        var baseUrl = _config["COORDENADOR_CONVITE_LINK"]
            ?? throw new InvalidOperationException("Variável de ambiente COORDENADOR_CONVITE_LINK não configurada.");

        var token = Guid.NewGuid().ToString();
        var link = $"{baseUrl}?token={token}&email={Uri.EscapeDataString(request.Email)}";
        var corpo = _templateService.RenderConviteCoordenador(link);

        // Envia o e-mail ANTES de persistir o convite.
        // Se o envio falhar, nenhum registro órfão fica no banco.
        await _emailService.EnviarEmailAsync(
            request.Email,
            "Convite para cadastro de Coordenador — hora+",
            corpo
        );

        var convite = new ConviteCoordenadorBuilder()
            .WithId(Guid.NewGuid())
            .WithEmail(request.Email)
            .WithCursoId(request.CursoId)
            .WithToken(token)
            .WithExpiracao(DateTime.UtcNow.AddHours(2))
            .Build();

        await _conviteRepo.AddAsync(convite);
        await _conviteRepo.SaveChangesAsync();
    }
}
