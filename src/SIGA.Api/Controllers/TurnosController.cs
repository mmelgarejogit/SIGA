using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Turnos;
using SIGA.Application.Interfaces;
using System.Security.Claims;

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
        [FromQuery] string? estado,
        [FromQuery] int? patientId,
        [FromQuery] DateOnly? desde,
        [FromQuery] DateOnly? hasta)
    {
        var result = await _turnoService.GetAllAsync(fecha, professionalId, estado, patientId, desde, hasta);
        return ToHttpResponse(result);
    }

    [HttpGet("disponibles")]
    [Authorize(Policy = "ver_disponibles")]
    public async Task<IActionResult> GetDisponibles(
        [FromQuery] int professionalId,
        [FromQuery] DateOnly fecha,
        [FromQuery] int? sucursalId)
    {
        var result = await _turnoService.GetSlotsDisponiblesAsync(professionalId, fecha, sucursalId);
        return ToHttpResponse(result);
    }

    /// <summary>Profesionales con horario activo en la fecha dada — para el flujo de reserva del paciente.</summary>
    [HttpGet("profesionales-disponibles")]
    [Authorize(Policy = "ver_mis_turnos")]
    public async Task<IActionResult> GetProfesionalesDisponibles(
        [FromQuery] DateOnly fecha,
        [FromQuery] int? sucursalId)
    {
        var result = await _turnoService.GetProfesionalesDisponiblesAsync(fecha, sucursalId);
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

    // ── Patient self-scheduling ───────────────────────────────────────────

    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet("mis-turnos")]
    [Authorize(Policy = "ver_mis_turnos")]
    public async Task<IActionResult> GetMisTurnos()
    {
        var result = await _turnoService.GetMisTurnosAsync(CurrentUserId);
        return ToHttpResponse(result);
    }

    [HttpPost("self-book")]
    [Authorize(Policy = "ver_mis_turnos")]
    public async Task<IActionResult> SelfBook([FromBody] SelfBookTurnoRequest request)
    {
        var result = await _turnoService.SelfBookAsync(CurrentUserId, request);
        return ToHttpResponse(result);
    }

    [HttpPost("{id:int}/solicitar-cancelacion")]
    [Authorize(Policy = "ver_mis_turnos")]
    public async Task<IActionResult> SolicitarCancelacion(int id)
    {
        var result = await _turnoService.SolicitarCancelacionAsync(id, CurrentUserId);
        return ToHttpResponse(result);
    }

    [HttpPost("{id:int}/gestionar-cancelacion")]
    [Authorize(Policy = "gestionar_agenda")]
    public async Task<IActionResult> GestionarCancelacion(int id, [FromBody] GestionarCancelacionRequest request)
    {
        var result = await _turnoService.GestionarCancelacionAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpPost("confirmar/{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmarPorToken(string token)
    {
        var result = await _turnoService.ConfirmarPorTokenAsync(token);
        return ToHttpResponse(result);
    }
}
