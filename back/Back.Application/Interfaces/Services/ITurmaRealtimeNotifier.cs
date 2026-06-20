using System;
using System.Threading.Tasks;

namespace Back.Application.Interfaces.Services;

/// <summary>
/// Notifica, em tempo real, que a lista de alunos de uma turma mudou — para que
/// os coordenadores com a turma aberta atualizem sem recarregar a página.
/// A implementação (SignalR) vive na camada de API; o caso de uso depende apenas
/// desta abstração.
/// </summary>
public interface ITurmaRealtimeNotifier
{
    Task NotificarAlunosAlteradosAsync(Guid turmaId);
}
