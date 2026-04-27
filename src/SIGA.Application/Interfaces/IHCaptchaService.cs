namespace SIGA.Application.Interfaces;

public interface IHCaptchaService
{
    Task<bool> VerifyAsync(string token);
}
