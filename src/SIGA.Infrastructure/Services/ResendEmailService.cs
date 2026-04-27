using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using SIGA.Application.Interfaces;
using SIGA.Infrastructure.Options;

namespace SIGA.Infrastructure.Services;

public class ResendEmailService : IEmailService
{
    private readonly IHttpClientFactory _factory;
    private readonly ResendOptions _options;

    public ResendEmailService(IHttpClientFactory factory, IOptions<ResendOptions> options)
    {
        _factory = factory;
        _options = options.Value;
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody)
    {
        var client = _factory.CreateClient("resend");

        var payload = new
        {
            from    = $"{_options.FromName} <{_options.FromEmail}>",
            to      = new[] { toEmail },
            subject,
            html    = htmlBody
        };

        var response = await client.PostAsJsonAsync("https://api.resend.com/emails", payload);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Resend {(int)response.StatusCode}: {body}");
        }
    }
}
