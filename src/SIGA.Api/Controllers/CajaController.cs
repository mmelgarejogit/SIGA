using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Ventas;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/caja")]
[Authorize(Policy = "ver_ventas")]
public class CajaController(ICajaService cajaService) : BaseController
{
    private int CurrentUserId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    // ── Resumen / movimientos (existentes) ────────────────────────────────────────

    [HttpGet("resumen")]
    public async Task<IActionResult> GetResumen([FromQuery] string fecha)
    {
        var result = await cajaService.GetResumenAsync(fecha);
        return ToHttpResponse(result);
    }

    [HttpGet("movimientos")]
    public async Task<IActionResult> GetMovimientos(
        [FromQuery] string? fechaDesde = null,
        [FromQuery] string? fechaHasta = null,
        [FromQuery] string? tipo = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await cajaService.GetMovimientosAsync(fechaDesde, fechaHasta, tipo, page, pageSize);
        return ToHttpResponse(result);
    }

    // ── Sesiones de caja ──────────────────────────────────────────────────────────

    [HttpGet("sesion-actual")]
    public async Task<IActionResult> GetSesionActual()
    {
        var result = await cajaService.GetSesionActualAsync();
        return ToHttpResponse(result);
    }

    [HttpGet("apertura-sugerida")]
    public async Task<IActionResult> GetAperturaSugerida()
        => ToHttpResponse(await cajaService.GetMontoAperturaSugeridoAsync());

    [HttpPost("sesiones")]
    [Authorize(Policy = "gestionar_caja")]
    public async Task<IActionResult> AbrirSesion([FromBody] AbrirSesionRequest request)
    {
        var result = await cajaService.AbrirSesionAsync(request, CurrentUserId);
        return ToHttpResponse(result);
    }

    [HttpGet("sesiones/{id:int}")]
    public async Task<IActionResult> GetSesion(int id)
    {
        var result = await cajaService.GetSesionByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost("sesiones/{id:int}/cerrar")]
    [Authorize(Policy = "gestionar_caja")]
    public async Task<IActionResult> CerrarSesion(int id, [FromBody] CerrarSesionRequest request)
    {
        var result = await cajaService.CerrarSesionAsync(id, request, CurrentUserId);
        return ToHttpResponse(result);
    }

    [HttpGet("sesiones")]
    public async Task<IActionResult> GetSesiones(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? estado = null)
    {
        var result = await cajaService.GetSesionesAsync(page, pageSize, estado);
        return ToHttpResponse(result);
    }

    [HttpPost("sesiones/{id:int}/aprobar-cierre")]
    [Authorize(Policy = "aprobar_cierres_caja")]
    public async Task<IActionResult> AprobarCierre(int id)
    {
        var result = await cajaService.AprobarCierreAsync(id, CurrentUserId);
        return ToHttpResponse(result);
    }

    [HttpPost("sesiones/{id:int}/rechazar-cierre")]
    [Authorize(Policy = "aprobar_cierres_caja")]
    public async Task<IActionResult> RechazarCierre(int id, [FromBody] RechazarCierreRequest request)
    {
        var result = await cajaService.RechazarCierreAsync(id, request, CurrentUserId);
        return ToHttpResponse(result);
    }
}
