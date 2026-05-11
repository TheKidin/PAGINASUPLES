using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace PIA.Services
{
    public class EmailSenderService
    {
        private readonly IConfiguration _config;

        public EmailSenderService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCorreoAsync(string destino, string asunto, string mensajeHtml)
        {
            var host = _config["SmtpConfig:Host"];
            var port = int.Parse(_config["SmtpConfig:Port"]);
            var userName = _config["SmtpConfig:UserName"];
            var password = _config["SmtpConfig:Password"];

            using var clienteSmtp = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(userName, password),
                EnableSsl = true // 🛡️ Conexión encriptada
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(userName, "SMUANL Performance"),
                Subject = asunto,
                Body = mensajeHtml,
                IsBodyHtml = true // Para que acepte diseño y colores
            };

            mailMessage.To.Add(destino);

            await clienteSmtp.SendMailAsync(mailMessage);
        }
    }
}