using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Back.API.Hubs;

/// <summary>
/// Hub de tempo real da turma. O cliente entra no grupo da turma que está
/// visualizando e o servidor empurra eventos apenas para esse grupo.
///
/// A mensagem NÃO carrega dados sensíveis — apenas sinaliza que a lista de alunos
/// mudou. O cliente, ao receber o sinal, refaz o fetch autenticado via REST.
/// </summary>
public class TurmaHub : Hub
{
    public static string GrupoDaTurma(string turmaId) => $"turma-{turmaId}";

    public Task EntrarTurma(string turmaId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GrupoDaTurma(turmaId));

    public Task SairTurma(string turmaId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GrupoDaTurma(turmaId));
}
