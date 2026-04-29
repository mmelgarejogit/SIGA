namespace SIGA.Infrastructure.Options;

public class MailgunOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Domain { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}
