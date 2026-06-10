using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/caja")]
[Authorize(Policy = "ver_ventas")]
public class CajaController(ICajaService cajaService) : BaseController
{
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

    [HttpDelete("movimientos/{id:int}")]
    [Authorize(Policy = "gestionar_ventas")]
    public async Task<IActionResult> DeleteMovimiento(int id)
    {
        var result = await cajaService.DeleteMovimientoAsync(id);
        return ToHttpResponse(result);
    }
}
