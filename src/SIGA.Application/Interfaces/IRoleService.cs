using SIGA.Application.Common;
using SIGA.Application.DTOs.Roles;

namespace SIGA.Application.Interfaces;

public interface IRoleService
{
    Task<Result<IEnumerable<RoleResponse>>> GetAllAsync();
    Task<Result<RoleResponse>> GetByIdAsync(int id);
    Task<Result<RoleResponse>> CreateAsync(RoleRequest request);
    Task<Result<RoleResponse>> UpdateAsync(int id, RoleRequest request);
    Task<Result<bool>> DeleteAsync(int id);

    Task<Result<bool>> AssignRoleToUserAsync(int userId, AssignRoleRequest request);
    Task<Result<bool>> RemoveRoleFromUserAsync(int userId, int roleId);
    Task<Result<IEnumerable<RoleResponse>>> GetRolesByUserAsync(int userId);
}
