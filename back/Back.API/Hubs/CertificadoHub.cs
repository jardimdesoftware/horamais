using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace Back.API.Hubs;

/// <summary>
/// Hub de tempo real de certificados. O coordenador entra no grupo do curso que
/// coordena e o servidor empurra o evento apenas para esse grupo.
///
/// A mensagem NÃO carrega dados sensíveis — apenas sinaliza que um novo
/// certificado chegou. O cliente, ao receber o sinal, exibe a notificação e
/// (ao abrir a validação) refaz o fetch autenticado via REST.
/// </summary>
public class CertificadoHub : Hub
{
    public static string GrupoDoCurso(string cursoId) => $"curso-{cursoId}";

    public Task EntrarCurso(string cursoId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GrupoDoCurso(cursoId));

    public Task SairCurso(string cursoId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GrupoDoCurso(cursoId));
}
