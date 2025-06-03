using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace EventOrganizer.Server.Tools
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpOptions)
        {
            _smtpSettings = smtpOptions.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var client = new SmtpClient(_smtpSettings.Host)
            {
                Port = _smtpSettings.Port,
                Credentials = new NetworkCredential(_smtpSettings.User, _smtpSettings.Password),
                EnableSsl = _smtpSettings.EnableSsl
            };

            var mail = new MailMessage(_smtpSettings.From, to, subject, body)
            {
                IsBodyHtml = true
            };
            // For now we do not send
            // await client.SendMailAsync(mail);
        }
    }
}