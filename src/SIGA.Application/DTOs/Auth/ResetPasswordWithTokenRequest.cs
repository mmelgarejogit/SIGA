namespace SIGA.Application.DTOs.Auth;

public class ResetPasswordWithTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
