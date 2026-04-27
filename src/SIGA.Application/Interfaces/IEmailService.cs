namespace SIGA.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string htmlBody);
}
