using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Turnos;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TurnosController : BaseController
{
    private readonly ITurnoService _turnoService;

    public TurnosController(ITurnoService turnoService)
    {
        _turnoService = turnoService;
    }

    [HttpGet]
    [Authorize(Policy = "ver_agenda")]
    public async Task<IActionResult> GetAll(
        [FromQuery] DateOnly? fecha,
        [FromQuery] int? professionalId,
        [FromQuery] string? estado)
    {
        var result = await _turnoService.GetAllAsync(fecha, professionalId, estado);
        return ToHttpResponse(result);
    }

    [HttpGet("disponibles")]
    [Authorize(Policy = "ver_agenda")]
    public async Task<IActionResult> GetDisponibles(
        [FromQuery] int professionalId,
        [FromQuery] DateOnly fecha)
    {
        var result = await _turnoService.GetSlotsDisponiblesAsync(professionalId, fecha);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "gestionar_agenda")]
    public async Task<IActionResult> Create([FromBody] CreateTurnoRequest request)
    {
        var result = await _turnoService.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}/estado")]
    [Authorize(Policy = "gestionar_agenda")]
    public async Task<IActionResult> UpdateEstado(int id, [FromBody] UpdateTurnoEstadoRequest request)
    {
        var result = await _turnoService.UpdateEstadoAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "gestionar_agenda")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _turnoService.CancelAsync(id);
        return ToHttpResponse(result);
    }
}
