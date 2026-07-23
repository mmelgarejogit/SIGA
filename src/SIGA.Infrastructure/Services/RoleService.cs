using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Roles;
using SIGA.Application.DTOs.Users;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly AppDbContext _dbContext;
    private readonly IAuditService _audit;

    public RoleService(AppDbContext dbContext, IAuditService audit)
    {
        _dbContext = dbContext;
        _audit     = audit;
    }

    private async Task<string> NombreUsuario(int userId) =>
        await _dbContext.Users
            .Where(u => u.Id == userId)
            .Select(u => (u.Person.FirstName + " " + u.Person.LastName).Trim())
            .FirstOrDefaultAsync() ?? $"usuario #{userId}";

    private async Task<string> NombreRol(int roleId) =>
        await _dbContext.Roles
            .Where(r => r.Id == roleId)
            .Select(r => r.Name)
            .FirstOrDefaultAsync() ?? $"rol #{roleId}";

    public async Task<Result<IEnumerable<RoleResponse>>> GetAllAsync()
    {
        var roles = await _dbContext.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .ToListAsync();
        return Result<IEnumerable<RoleResponse>>.Success(roles.Select(ToResponse));
    }

    public async Task<Result<RoleResponse>> GetByIdAsync(int id)
    {
        var role = await _dbContext.Roles
            .Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role is null)
            return Result<RoleResponse>.Failure("Role not found.", ErrorType.NotFound);

        return Result<RoleResponse>.Success(ToResponse(role));
    }

    public async Task<Result<RoleResponse>> CreateAsync(RoleRequest request)
    {
        var name = request.Name.Trim();

        if (await _dbContext.Roles.AnyAsync(r => r.Name == name))
            return Result<RoleResponse>.Failure("Role name already exists.", ErrorType.Conflict);

        var role = new Role { Name = name };
        _dbContext.Roles.Add(role);
        await _dbContext.SaveChangesAsync();

        await SetPermissionsAsync(role.Id, request.Permissions);

        await _audit.LogAsync(AuditAccion.RolCreado, $"Creó el rol {name}",
            entidad: "Role", entidadId: role.Id);

        return await GetByIdAsync(role.Id);
    }

    public async Task<Result<RoleResponse>> UpdateAsync(int id, RoleRequest request)
    {
        var role = await _dbContext.Roles.FindAsync(id);
        if (role is null)
            return Result<RoleResponse>.Failure("Role not found.", ErrorType.NotFound);

        var name = request.Name.Trim();

        if (await _dbContext.Roles.AnyAsync(r => r.Name == name && r.Id != id))
            return Result<RoleResponse>.Failure("Role name already exists.", ErrorType.Conflict);

        role.Name = name;
        await _dbContext.SaveChangesAsync();

        await SetPermissionsAsync(id, request.Permissions);

        await _audit.LogAsync(AuditAccion.RolActualizado,
            $"Editó el rol {name} (nombre y permisos)",
            entidad: "Role", entidadId: id);

        return await GetByIdAsync(id);
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var role = await _dbContext.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (role is null)
            return Result<bool>.Failure("Role not found.", ErrorType.NotFound);

        if (role.UserRoles.Count > 0)
            return Result<bool>.Failure("Cannot delete a role that is assigned to users.", ErrorType.Conflict);

        _dbContext.Roles.Remove(role);
        await _dbContext.SaveChangesAsync();

        await _audit.LogAsync(AuditAccion.RolEliminado, $"Eliminó el rol {role.Name}",
            entidad: "Role", entidadId: id);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> AssignRoleToUserAsync(int userId, AssignRoleRequest request)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user is null)
            return Result<bool>.Failure("User not found.", ErrorType.NotFound);

        var role = await _dbContext.Roles.FindAsync(request.RoleId);
        if (role is null)
            return Result<bool>.Failure("Role not found.", ErrorType.NotFound);

        var alreadyAssigned = await _dbContext.UserRoles
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == request.RoleId);

        if (alreadyAssigned)
            return Result<bool>.Failure("User already has this role.", ErrorType.Conflict);

        _dbContext.UserRoles.Add(new UserRole { UserId = userId, RoleId = request.RoleId });
        await _dbContext.SaveChangesAsync();

        await _audit.LogAsync(AuditAccion.RolAsignado,
            $"Asignó el rol {role.Name} a {await NombreUsuario(userId)}",
            entidad: "User", entidadId: userId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> RemoveRoleFromUserAsync(int userId, int roleId)
    {
        var userRole = await _dbContext.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

        if (userRole is null)
            return Result<bool>.Failure("Assignment not found.", ErrorType.NotFound);

        _dbContext.UserRoles.Remove(userRole);
        await _dbContext.SaveChangesAsync();

        await _audit.LogAsync(AuditAccion.RolQuitado,
            $"Quitó el rol {await NombreRol(roleId)} a {await NombreUsuario(userId)}",
            entidad: "User", entidadId: userId);

        return Result<bool>.Success(true);
    }

    public async Task<Result<IEnumerable<RoleResponse>>> GetRolesByUserAsync(int userId)
    {
        var userExists = await _dbContext.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
            return Result<IEnumerable<RoleResponse>>.Failure("User not found.", ErrorType.NotFound);

        var roles = await _dbContext.UserRoles
            .Where(ur => ur.UserId == userId)
            .Include(ur => ur.Role).ThenInclude(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .Select(ur => ur.Role)
            .ToListAsync();

        return Result<IEnumerable<RoleResponse>>.Success(roles.Select(ToResponse));
    }

    public async Task<Result<IEnumerable<UserResponse>>> GetUsersByRoleAsync(int roleId)
    {
        var roleExists = await _dbContext.Roles.AnyAsync(r => r.Id == roleId);
        if (!roleExists)
            return Result<IEnumerable<UserResponse>>.Failure("Role not found.", ErrorType.NotFound);

        var users = await _dbContext.Users
            .Where(u => u.UserRoles.Any(ur => ur.RoleId == roleId))
            .Include(u => u.Person)
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.Professional)
            .Include(u => u.Patient)
            .Include(u => u.Sucursal)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return Result<IEnumerable<UserResponse>>.Success(users.Select(ToUserResponse));
    }

    private static UserResponse ToUserResponse(User u)
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
            UserId         = u.Id,
            PersonId       = u.PersonId,
            CI             = u.Person.CI,
            FirstName      = u.Person.FirstName,
            LastName       = u.Person.LastName,
            Email          = u.Person.Email,
            PhoneNumber    = u.Person.PhoneNumber,
            IsActive       = u.IsActive,
            Type           = type,
            SucursalId     = u.SucursalId,
            SucursalNombre = u.Sucursal?.Nombre,
            Roles          = u.UserRoles.Select(ur => ur.Role.Name).ToList(),
            CreatedAt      = u.CreatedAt,
        };
    }

    private async Task SetPermissionsAsync(int roleId, List<string> permissionNames)
    {
        var existing = await _dbContext.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
        _dbContext.RolePermissions.RemoveRange(existing);

        foreach (var name in permissionNames.Select(p => p.Trim()).Distinct())
        {
            var permission = await _dbContext.Permissions.FirstOrDefaultAsync(p => p.Name == name)
                             ?? new Permission { Name = name };

            if (permission.Id == 0)
                _dbContext.Permissions.Add(permission);

            await _dbContext.SaveChangesAsync();

            _dbContext.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = permission.Id });
        }

        await _dbContext.SaveChangesAsync();
    }

    private static RoleResponse ToResponse(Role r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Type = r.Type,
        Permissions = r.RolePermissions.Select(rp => rp.Permission.Name).ToList()
    };
}
