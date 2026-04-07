namespace SIGA.Application.Interfaces;

using SIGA.Application.DTOs.Auth;
using SIGA.Domain.Entities;

public interface IAuthService
{
    public Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    public Task<LoginResponse> LoginAsync(LoginRequest request);
}