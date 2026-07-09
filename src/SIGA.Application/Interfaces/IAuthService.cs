namespace SIGA.Application.Interfaces;

using SIGA.Application.Common;
using SIGA.Application.DTOs.Auth;

public interface IAuthService
{
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<RegisterResponse>> RegisterPatientAsync(RegisterPatientRequest request);
    Task<Result<bool>> VerifyEmailAsync(string token);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
    Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest request);
    Task<Result<bool>> RequestPasswordResetAsync(ForgotPasswordRequest request);
    Task<Result<bool>> ResetPasswordWithTokenAsync(ResetPasswordWithTokenRequest request);
}