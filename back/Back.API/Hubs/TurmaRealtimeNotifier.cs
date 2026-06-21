using System;
using System.Threading.Tasks;
using Back.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace Back.API.Hubs;

/// <summary>
/// Implementação SignalR de <see cref="ITurmaRealtimeNotifier"/>. Empurra o evento
/// "AlunosAlterados" apenas para o grupo da turma afetada.
/// </summary>
public class TurmaRealtimeNotifier : ITurmaRealtimeNotifier
{
    private readonly IHubContext<TurmaHub> _hub;

    public TurmaRealtimeNotifier(IHubContext<TurmaHub> hub) => _hub = hub;

    public Task NotificarAlunosAlteradosAsync(Guid turmaId) =>
        _hub.Clients
            .Group(TurmaHub.GrupoDaTurma(turmaId.ToString()))
            .SendAsync("AlunosAlterados", turmaId);
}
