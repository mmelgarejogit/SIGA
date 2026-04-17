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
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        var response = users.Select(u =>
        {
            string type;
            if (u.Professional is not null)
                type = "Profesional";
            else if (u.Patient is not null)
                type = "Paciente";
            else if (u.UserRoles.Any(ur => ur.Role.Name == "Admin"))
                type = "Administrador";
            else
                type = "Sin rol";

            return new UserResponse
            {
                UserId      = u.Id,
                PersonId    = u.PersonId,
                FirstName   = u.Person.FirstName,
                LastName    = u.Person.LastName,
                DNI         = u.Person.DNI,
                Email       = u.Person.Email,
                PhoneNumber = u.Person.PhoneNumber,
                IsActive    = u.IsActive,
                Type        = type,
                Roles       = u.UserRoles.Select(ur => ur.Role.Name),
                CreatedAt   = u.CreatedAt,
            };
        });

        return Result<IEnumerable<UserResponse>>.Success(response);
    }
}
