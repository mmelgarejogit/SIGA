using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using SIGA.Application.Interfaces;
using SIGA.Infrastructure.Options;

namespace SIGA.Infrastructure.Services;

public class HCaptchaService : IHCaptchaService
{
    private readonly IHttpClientFactory _factory;
    private readonly HCaptchaOptions _options;

    public HCaptchaService(IHttpClientFactory factory, IOptions<HCaptchaOptions> options)
    {
        _factory = factory;
        _options = options.Value;
    }

    public async Task<bool> VerifyAsync(string token)
    {
        var client = _factory.CreateClient("hcaptcha");

        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "secret",   _options.SecretKey },
            { "response", token              }
        });

        var response = await client.PostAsync("https://api.hcaptcha.com/siteverify", content);
        if (!response.IsSuccessStatusCode) return false;

        var result = await response.Content.ReadFromJsonAsync<HCaptchaResponse>();
        return result?.Success ?? false;
    }

    private class HCaptchaResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}
