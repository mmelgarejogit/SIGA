using Microsoft.Extensions.Options;
using SIGA.Application.Interfaces;
using SIGA.Infrastructure.Options;

namespace SIGA.Infrastructure.Services;

public class MailgunEmailService : IEmailService
{
    private readonly IHttpClientFactory _factory;
    private readonly MailgunOptions _options;

    public MailgunEmailService(IHttpClientFactory factory, IOptions<MailgunOptions> options)
    {
        _factory = factory;
        _options = options.Value;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var client = _factory.CreateClient("mailgun");

        var form = new MultipartFormDataContent
        {
            { new StringContent($"{_options.FromName} <{_options.FromEmail}>"), "from" },
            { new StringContent(toEmail),  "to"      },
            { new StringContent(subject),  "subject" },
            { new StringContent(htmlBody), "html"    }
        };

        var response = await client.PostAsync(
            $"https://api.mailgun.net/v3/{_options.Domain}/messages", form);

        response.EnsureSuccessStatusCode();
    }
}
