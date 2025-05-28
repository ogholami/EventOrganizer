using System.Net.Mail;
using System.Net;

namespace EventOrganizer.Server.Tools
{
    public class EmailService : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpClient = new SmtpClient("smtp.example.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("your@email.com", "yourpassword"),
                EnableSsl = true,
            };

            var mail = new MailMessage("your@email.com", to, subject, body);
            await smtpClient.SendMailAsync(mail);
        }
    }
}

