using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Users;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Domain.Security;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditService _audit;

    public UserService(AppDbContext dbContext, IPasswordHasher passwordHasher, IAuditService audit)
    {
        _dbContext      = dbContext;
        _passwordHasher = passwordHasher;
        _audit          = audit;
    }

    private async Task<string> NombreDe(int personId) =>
        await _dbContext.Persons
            .Where(p => p.Id == personId)
            .Select(p => (p.FirstName + " " + p.LastName).Trim())
            .FirstOrDefaultAsync() ?? $"usuario #{personId}";

    public async Task<Result<IEnumerable<UserResponse>>> GetAllAsync()
    {
        var users = await _dbContext.Users
            .Include(u => u.Person)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Professional)
            .Include(u => u.Patient)
            .Include(u => u.Sucursal)
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
                type = "Usuario";

            return new UserResponse
            {
                UserId      = u.Id,
                PersonId    = u.PersonId,
                CI          = u.Person.CI,
                FirstName   = u.Person.FirstName,
                LastName    = u.Person.LastName,
                Email       = u.Person.Email,
                PhoneNumber = u.Person.PhoneNumber,
                IsActive       = u.IsActive,
                Type           = type,
                SucursalId     = u.SucursalId,
                SucursalNombre = u.Sucursal?.Nombre,
                Roles          = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
                CreatedAt      = u.CreatedAt,
            };
        });

        return Result<IEnumerable<UserResponse>>.Success(response);
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

        await _audit.LogAsync(AuditAccion.UsuarioDesactivado,
            $"Desactivó al usuario {await NombreDe(user.PersonId)}",
            entidad: "User", entidadId: user.Id);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> ResetPasswordAsync(int id, string newPassword)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user is null)
            return Result<bool>.Failure("Usuario no encontrado.", ErrorType.NotFound);

        var passwordError = PasswordPolicy.ValidateNew(newPassword, user.PasswordHash, _passwordHasher);
        if (passwordError is not null)
            return Result<bool>.Failure(passwordError, ErrorType.Validation);

        user.PasswordHash       = _passwordHasher.Hash(newPassword);
        user.MustChangePassword = true;
        user.UpdatedAt          = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        await _audit.LogAsync(AuditAccion.PasswordReseteado,
            $"Reseteó la contraseña del usuario {await NombreDe(user.PersonId)}",
            entidad: "User", entidadId: user.Id);

        return Result<bool>.Success(true);
    }
}
