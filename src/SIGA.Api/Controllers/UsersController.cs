using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = "ver_usuarios")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllAsync();
        return ToHttpResponse(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "editar_usuario")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await _userService.DeactivateAsync(id);
        return ToHttpResponse(result);
    }
}
