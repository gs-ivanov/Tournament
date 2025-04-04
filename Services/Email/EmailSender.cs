using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Tournament.Services.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string toEmail, string subject, string body)
        {
            var config = _configuration.GetSection("GmailSettings");
            var smtpHost = config["Host"];
            var smtpPort = int.Parse(config["Port"]);
            var fromEmail = config["FromEmail"];
            var username = config["Username"];
            var password = config["Password"];

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            using var message = new MailMessage(fromEmail, toEmail, subject, body);
           

            try
            {
                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                // Лог или временно отстраняване на грешката
                throw new InvalidOperationException($"Грешка при изпращане на имейл: {ex.Message}", ex);
            }

        }
    }
}
