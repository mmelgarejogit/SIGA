using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize]
public class ReportesController(IReporteService reporteService) : BaseController
{
    /// <summary>
    /// Reporte de ventas agregado para un rango de fechas.
    /// <paramref name="agrupacion"/>: "dia" | "semana" | "mes" (controla los buckets de la serie temporal).
    /// </summary>
    [HttpGet("ventas")]
    [Authorize(Policy = "ver_reportes")]
    public async Task<IActionResult> GetReporteVentas(
        [FromQuery] string desde,
        [FromQuery] string hasta,
        [FromQuery] string agrupacion = "dia")
    {
        if (!DateOnly.TryParse(desde, out var d) || !DateOnly.TryParse(hasta, out var h))
            return BadRequest("Fechas inválidas (formato esperado: yyyy-MM-dd).");
        if (h < d)
            return BadRequest("La fecha 'hasta' no puede ser anterior a 'desde'.");

        var result = await reporteService.GetReporteVentasAsync(d, h, agrupacion);
        return ToHttpResponse(result);
    }

    /// <summary>
    /// Reporte de citas (turnos, consultas y recetas) agregado para un rango de fechas.
    /// <paramref name="agrupacion"/>: "dia" | "semana" | "mes" (controla los buckets de la serie temporal).
    /// </summary>
    [HttpGet("citas")]
    [Authorize(Policy = "ver_reportes")]
    public async Task<IActionResult> GetReporteCitas(
        [FromQuery] string desde,
        [FromQuery] string hasta,
        [FromQuery] string agrupacion = "dia")
    {
        if (!DateOnly.TryParse(desde, out var d) || !DateOnly.TryParse(hasta, out var h))
            return BadRequest("Fechas inválidas (formato esperado: yyyy-MM-dd).");
        if (h < d)
            return BadRequest("La fecha 'hasta' no puede ser anterior a 'desde'.");

        var result = await reporteService.GetReporteCitasAsync(d, h, agrupacion);
        return ToHttpResponse(result);
    }

    /// <summary>
    /// Reporte de inventario: snapshot de stock (valorización, crítico, por categoría) y
    /// movimientos aprobados del rango. <paramref name="agrupacion"/>: "dia" | "semana" | "mes".
    /// </summary>
    [HttpGet("inventario")]
    [Authorize(Policy = "ver_reportes")]
    public async Task<IActionResult> GetReporteInventario(
        [FromQuery] string desde,
        [FromQuery] string hasta,
        [FromQuery] string agrupacion = "dia")
    {
        if (!DateOnly.TryParse(desde, out var d) || !DateOnly.TryParse(hasta, out var h))
            return BadRequest("Fechas inválidas (formato esperado: yyyy-MM-dd).");
        if (h < d)
            return BadRequest("La fecha 'hasta' no puede ser anterior a 'desde'.");

        var result = await reporteService.GetReporteInventarioAsync(d, h, agrupacion);
        return ToHttpResponse(result);
    }

    /// <summary>
    /// Reporte de compras: órdenes de compra, facturas (monto, IVA, pendiente de pago),
    /// recepciones y compras por proveedor del rango. Permiso del módulo de compras.
    /// </summary>
    [HttpGet("compras")]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetReporteCompras(
        [FromQuery] string desde,
        [FromQuery] string hasta,
        [FromQuery] string agrupacion = "dia")
    {
        if (!DateOnly.TryParse(desde, out var d) || !DateOnly.TryParse(hasta, out var h))
            return BadRequest("Fechas inválidas (formato esperado: yyyy-MM-dd).");
        if (h < d)
            return BadRequest("La fecha 'hasta' no puede ser anterior a 'desde'.");

        var result = await reporteService.GetReporteComprasAsync(d, h, agrupacion);
        return ToHttpResponse(result);
    }
}
