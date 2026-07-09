namespace SIGA.Application.DTOs.Auth;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
    public string HCaptchaToken { get; set; } = string.Empty;
}
