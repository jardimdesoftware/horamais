using System;
using System.Threading.Tasks;
using Back.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace Back.API.Hubs;

/// <summary>
/// Implementação SignalR de <see cref="ICertificadoRealtimeNotifier"/>. Empurra o
/// evento "NovoCertificado" apenas para o grupo do curso afetado.
/// </summary>
public class CertificadoRealtimeNotifier : ICertificadoRealtimeNotifier
{
    private readonly IHubContext<CertificadoHub> _hub;

    public CertificadoRealtimeNotifier(IHubContext<CertificadoHub> hub) => _hub = hub;

    public Task NotificarNovoCertificadoAsync(Guid cursoId) =>
        _hub.Clients
            .Group(CertificadoHub.GrupoDoCurso(cursoId.ToString()))
            .SendAsync("NovoCertificado", cursoId);
}
