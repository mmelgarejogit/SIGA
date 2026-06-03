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
            .Include(u => u.Sucursal)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Professional)
            .Include(u => u.Patient)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return Result<IEnumerable<UserResponse>>.Success(users.Select(ToResponse));
    }

    public async Task<Result<bool>> DeactivateAsync(int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user is null)
            return Result<bool>.Failure("User not found.", ErrorType.NotFound);

        if (!user.IsActive)
            return Result<bool>.Failure("User is already inactive.", ErrorType.Conflict);

        user.IsActive  = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    public async Task<Result<UserResponse>> AssignSucursalAsync(int userId, Guid? sucursalId)
    {
        var user = await _dbContext.Users
            .Include(u => u.Person)
            .Include(u => u.Sucursal)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user is null)
            return Result<UserResponse>.Failure("Usuario no encontrado.", ErrorType.NotFound);

        if (sucursalId.HasValue)
        {
            var sucursal = await _dbContext.Sucursales.FindAsync(sucursalId.Value);
            if (sucursal is null || !sucursal.IsActive)
                return Result<UserResponse>.Failure("Sucursal no encontrada o inactiva.", ErrorType.NotFound);
            user.SucursalId = sucursalId;
            user.Sucursal   = sucursal;
        }
        else
        {
            user.SucursalId = null;
            user.Sucursal   = null;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return Result<UserResponse>.Success(ToResponse(user));
    }

    private static UserResponse ToResponse(SIGA.Domain.Entities.User u)
    {
        string type;
        if (u.Professional is not null)       type = "Profesional";
        else if (u.Patient is not null)        type = "Paciente";
        else if (u.UserRoles.Any(ur => ur.Role.Name == "Administrador")) type = "Administrador";
        else                                   type = "Usuario";

        return new UserResponse
        {
            UserId        = u.Id,
            PersonId      = u.PersonId,
            CI            = u.Person.CI,
            FirstName     = u.Person.FirstName,
            LastName      = u.Person.LastName,
            Email         = u.Person.Email,
            PhoneNumber   = u.Person.PhoneNumber,
            IsActive      = u.IsActive,
            Type          = type,
            Roles         = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            SucursalId    = u.SucursalId,
            SucursalNombre = u.Sucursal?.Nombre,
            CreatedAt     = u.CreatedAt,
        };
    }
}
