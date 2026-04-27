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
        if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            return Result<RegisterResponse>.Failure("First name and last name are required.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return Result<RegisterResponse>.Failure("Email and password are required.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.CI))
            return Result<RegisterResponse>.Failure("CI is required.", ErrorType.Validation);

        var email = request.Email.Trim().ToLower();

        if (await _dbContext.Persons.AnyAsync(p => p.Email == email))
            return Result<RegisterResponse>.Failure("Email is already in use.", ErrorType.Conflict);

        if (await _dbContext.Persons.AnyAsync(p => p.CI == request.CI.Trim()))
            return Result<RegisterResponse>.Failure("CI is already in use.", ErrorType.Conflict);

        var now = DateTime.UtcNow;

        var person = new Person
        {
            CI = request.CI.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            BirthDate = request.BirthDate,
            PhoneNumber = request.PhoneNumber?.Trim(),
            Email = email,
            CreatedAt = now,
            UpdatedAt = now
        };

        var user = new User
        {
            Person = person,
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        return Result<RegisterResponse>.Success(new RegisterResponse
        {
            Email = person.Email,
            FirstName = person.FirstName,
            LastName = person.LastName
        });
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().ToLower();

        var user = await _dbContext.Users
            .Include(u => u.Person)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
                .ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Include(u => u.Professional)
                .ThenInclude(p => p!.Especialidades).ThenInclude(pe => pe.Especialidad)
            .FirstOrDefaultAsync(u => u.Person.Email == email);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return Result<LoginResponse>.Failure("Invalid email or password.", ErrorType.Unauthorized);

        if (!user.IsActive)
            return Result<LoginResponse>.Failure("Account is disabled.", ErrorType.Unauthorized);

        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.Name))
            .Distinct()
            .ToList();
        var token = _jwtTokenGenerator.GenerateToken(user, roles, permissions);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            JwtToken    = token,
            Email       = user.Person.Email,
            FirstName   = user.Person.FirstName,
            LastName    = user.Person.LastName,
            Specialty   = user.Professional?.Especialidades.FirstOrDefault()?.Especialidad.Nombre,
            RoleClaims  = roles,
            Permissions = permissions
        });
    }
}
