using System.Threading.Tasks;

namespace Back.Application.Interfaces;

public interface IEmailService
{
    Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml);
}
