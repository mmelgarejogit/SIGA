using SIGA.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using SIGA.Infrastructure.Persistence;
using SIGA.Domain.Security;
using SIGA.Domain.Entities;
using SIGA.Application.DTOs.Auth;

namespace SIGA.Application.Services;
public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    public AuthService (AppDbContext dbContext, IPasswordHasher passwordHasher){
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        if(string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrEmpty(request.LastName))
        {
            throw new ArgumentException("First Name and Last Name are required.");
        }

        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            throw new ArgumentException("Email and Password are required.");
        }

        var email = request.Email.Trim().ToLower();
        var existingEmail = await _dbContext.Users.AnyAsync(u => u.Email == email);
        if (existingEmail)
        {
            throw new InvalidOperationException("Email is already in use.");
        }

        var hashedPassword = _passwordHasher.Hash(request.Password);

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = hashedPassword,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();
        return new RegisterResponse
        {
            Email = user.Email,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty
        };
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request) {
        throw new NotImplementedException();
    }
};

