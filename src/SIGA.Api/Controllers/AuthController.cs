using Microsoft.AspNetCore.Mvc;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Auth;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        return ToHttpResponse(result);
    }
}

[ApiController]
public class BaseController : ControllerBase
{
    protected IActionResult ToHttpResponse<T>(Result<T> result)
    {
        if (result.IsSuccess) return Ok(result.Value);

        return result.ErrorType switch
        {
            ErrorType.Validation   => BadRequest(result.Error),
            ErrorType.Conflict     => Conflict(result.Error),
            ErrorType.NotFound     => NotFound(result.Error),
            ErrorType.Unauthorized => Unauthorized(result.Error),
            _                      => StatusCode(500, result.Error)
        };
    }
}
