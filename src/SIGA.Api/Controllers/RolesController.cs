using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Roles;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : BaseController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [Authorize(Policy = "ver_roles")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _roleService.GetAllAsync();
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "ver_roles")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _roleService.GetByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "crear_rol")]
    public async Task<IActionResult> Create([FromBody] RoleRequest request)
    {
        var result = await _roleService.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "editar_rol")]
    public async Task<IActionResult> Update(int id, [FromBody] RoleRequest request)
    {
        var result = await _roleService.UpdateAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "eliminar_rol")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _roleService.DeleteAsync(id);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}/users")]
    [Authorize(Policy = "ver_roles")]
    public async Task<IActionResult> GetUsersByRole(int id)
    {
        var result = await _roleService.GetRolesByUserAsync(id);
        return ToHttpResponse(result);
    }
}

[ApiController]
[Route("api/users/{userId:int}/roles")]
[Authorize]
public class UserRolesController : BaseController
{
    private readonly IRoleService _roleService;

    public UserRolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [Authorize(Policy = "ver_usuarios")]
    public async Task<IActionResult> GetRolesByUser(int userId)
    {
        var result = await _roleService.GetRolesByUserAsync(userId);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "editar_usuario")]
    public async Task<IActionResult> AssignRole(int userId, [FromBody] AssignRoleRequest request)
    {
        var result = await _roleService.AssignRoleToUserAsync(userId, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{roleId:int}")]
    [Authorize(Policy = "editar_usuario")]
    public async Task<IActionResult> RemoveRole(int userId, int roleId)
    {
        var result = await _roleService.RemoveRoleFromUserAsync(userId, roleId);
        return ToHttpResponse(result);
    }
}
