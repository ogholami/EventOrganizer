namespace EventOrganizer.Server.Tools
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
