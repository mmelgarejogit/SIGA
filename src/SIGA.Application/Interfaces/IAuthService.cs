namespace SIGA.Application.Interfaces;

using SIGA.Application.Common;
using SIGA.Application.DTOs.Auth;

public interface IAuthService
{
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
}