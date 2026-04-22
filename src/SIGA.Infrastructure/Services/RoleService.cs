using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Roles;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly AppDbContext _dbContext;

    public RoleService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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
        Permissions = r.RolePermissions.Select(rp => rp.Permission.Name).ToList()
    };
}
