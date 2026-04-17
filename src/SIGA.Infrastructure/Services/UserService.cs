using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Users;
using SIGA.Application.Interfaces;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;

    public UserService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<UserResponse>>> GetAllAsync()
    {
        var users = await _dbContext.Users
            .Include(u => u.Person)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Professional)
            .Include(u => u.Patient)
            .OrderBy(u => u.Person.LastName)
            .ToListAsync();

        var response = users.Select(u => new UserResponse
        {
            UserId    = u.Id,
            CI        = u.Person.CI,
            FirstName = u.Person.FirstName,
            LastName  = u.Person.LastName,
            Email     = u.Person.Email ?? string.Empty,
            Type      = u.Professional != null ? "Profesional"
                      : u.Patient      != null ? "Paciente"
                                               : "Usuario",
            IsActive  = u.IsActive,
            CreatedAt = u.CreatedAt,
            Roles     = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
        });

        return Result<IEnumerable<UserResponse>>.Success(response);
    }
}
