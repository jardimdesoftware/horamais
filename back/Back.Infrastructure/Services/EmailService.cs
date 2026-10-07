using Back.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using System;
using System.Threading.Tasks;

namespace Back.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public EmailService(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public async Task EnviarEmailAsync(string destinatario, string assunto, string corpoHtml)
        {
            var remetente = _configuration["Email:Remetente"];
            var senha = _configuration["Email:Senha"]?.Replace(" ", "");
            var smtp = _configuration["Email:Smtp"];
            var portaStr = _configuration["Email:Porta"];
            var localSmtp = _environment.IsDevelopment() &&
                bool.TryParse(_configuration["Email:LocalSmtp"], out var enabled) && enabled;

            if (string.IsNullOrWhiteSpace(remetente) || (!localSmtp && string.IsNullOrWhiteSpace(senha)) || string.IsNullOrWhiteSpace(smtp) || string.IsNullOrWhiteSpace(portaStr))
                throw new InvalidOperationException("Configurações de e-mail (Remetente, Senha, Smtp, Porta) ausentes ou inválidas no appsettings.");

            if (!int.TryParse(portaStr, out int porta))
                throw new InvalidOperationException("A porta configurada para o SMTP é inválida.");

            using var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(MailboxAddress.Parse(remetente));
            mimeMessage.To.Add(MailboxAddress.Parse(destinatario));
            mimeMessage.Subject = assunto;

            var builder = new BodyBuilder
            {
                HtmlBody = corpoHtml
            };
            mimeMessage.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            
            // Connect to the SMTP server
            await client.ConnectAsync(smtp, porta, localSmtp ? SecureSocketOptions.None : SecureSocketOptions.StartTls);
            
            // Authenticate using the app password
            if (!localSmtp)
                await client.AuthenticateAsync(remetente, senha!);
            
            // Send the email
            await client.SendAsync(mimeMessage);
            
            // Disconnect
            await client.DisconnectAsync(true);
        }
    }
}
