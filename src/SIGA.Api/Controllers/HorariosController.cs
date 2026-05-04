using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Horarios;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/professionals/{professionalId:int}")]
[Authorize]
public class HorariosController : BaseController
{
    private readonly IHorarioProfesionalService _horarioService;

    public HorariosController(IHorarioProfesionalService horarioService)
    {
        _horarioService = horarioService;
    }

    [HttpGet("horarios")]
    [Authorize(Policy = "ver_profesionales")]
    public async Task<IActionResult> GetHorarios(int professionalId)
    {
        var result = await _horarioService.GetHorariosAsync(professionalId);
        return ToHttpResponse(result);
    }

    [HttpPut("horarios")]
    [Authorize(Policy = "editar_profesional")]
    public async Task<IActionResult> SetHorarios(int professionalId, [FromBody] SetHorariosRequest request)
    {
        var result = await _horarioService.SetHorariosAsync(professionalId, request);
        return ToHttpResponse(result);
    }

    [HttpGet("bloqueos")]
    [Authorize(Policy = "ver_profesionales")]
    public async Task<IActionResult> GetBloqueos(int professionalId)
    {
        var result = await _horarioService.GetBloqueosAsync(professionalId);
        return ToHttpResponse(result);
    }

    [HttpPost("bloqueos")]
    [Authorize(Policy = "editar_profesional")]
    public async Task<IActionResult> AddBloqueo(int professionalId, [FromBody] BloqueoFechaRequest request)
    {
        var result = await _horarioService.AddBloqueoAsync(professionalId, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("bloqueos/{bloqueoId:int}")]
    [Authorize(Policy = "editar_profesional")]
    public async Task<IActionResult> RemoveBloqueo(int professionalId, int bloqueoId)
    {
        var result = await _horarioService.RemoveBloqueoAsync(professionalId, bloqueoId);
        return ToHttpResponse(result);
    }
}
