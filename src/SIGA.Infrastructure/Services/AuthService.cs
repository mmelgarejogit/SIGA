using SIGA.Application.Common;
using SIGA.Application.DTOs.Auth;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Domain.Security;
using SIGA.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SIGA.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public AuthService(AppDbContext dbContext, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrEmpty(request.LastName))
            return Result<RegisterResponse>.Failure("First Name and Last Name are required.", ErrorType.Validation);

        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return Result<RegisterResponse>.Failure("Email and Password are required.", ErrorType.Validation);

        var email = request.Email.Trim().ToLower();
        if (await _dbContext.Users.AnyAsync(u => u.Email == email))
            return Result<RegisterResponse>.Failure("Email is already in use.", ErrorType.Conflict);

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return Result<RegisterResponse>.Success(new RegisterResponse
        {
            Email = user.Email,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty
        });
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();
        var user = await _dbContext.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<LoginResponse>.Failure("Invalid email or password.", ErrorType.Unauthorized);

        if (!user.IsActive)
            return Result<LoginResponse>.Failure("Account is disabled.", ErrorType.Unauthorized);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var token = _jwtTokenGenerator.GenerateToken(user, roles);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            Email = user.Email,
            JwtToken = token,
            RoleClaims = roles
        });
    }
}
