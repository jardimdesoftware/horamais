using System;
using System.Threading.Tasks;

namespace Back.Application.Interfaces.Services;

/// <summary>
/// Notifica, em tempo real, que um novo certificado chegou para análise em um
/// curso — para que o coordenador responsável veja o aviso em qualquer tela,
/// sem precisar estar na validação. A implementação (SignalR) vive na camada de
/// API; o caso de uso depende apenas desta abstração.
/// </summary>
public interface ICertificadoRealtimeNotifier
{
    Task NotificarNovoCertificadoAsync(Guid cursoId);
}
