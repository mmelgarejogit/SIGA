using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Reportes;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/reportes")]
[Authorize]
public class ReportesController(
    IReporteService reporteService,
    IReporteOperativoService opService,
    IReporteExporter exporter) : BaseController
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
    [Authorize(Policy = "ver_reportes")]
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

    // ── Reportes operativos (listados filtrables + exportación) ───────────────────────

    /// <summary>
    /// Listado operativo paginado. <paramref name="tipo"/>: ventas | compras | inventario | caja.
    /// Filtros combinables (todos opcionales): desde, hasta, sucursalId, metodoPago, categoria,
    /// operadorId, tipoMov (Entrada/Salida o Ingreso/Egreso).
    /// </summary>
    [HttpGet("operativo/{tipo}")]
    [Authorize(Policy = "ver_reportes")]
    public async Task<IActionResult> GetOperativo(
        string tipo,
        [FromQuery] string? desde, [FromQuery] string? hasta,
        [FromQuery] int? sucursalId, [FromQuery] string? metodoPago,
        [FromQuery] string? categoria, [FromQuery] int? operadorId,
        [FromQuery] string? tipoMov, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var f = ParseFiltros(desde, hasta, sucursalId, metodoPago, categoria, operadorId, tipoMov, page, pageSize);
        return tipo.ToLowerInvariant() switch
        {
            "ventas"     => ToHttpResponse(await opService.GetVentasAsync(f)),
            "compras"    => ToHttpResponse(await opService.GetComprasAsync(f)),
            "inventario" => ToHttpResponse(await opService.GetMovInventarioAsync(f)),
            "caja"       => ToHttpResponse(await opService.GetMovCajaAsync(f)),
            _            => BadRequest("Tipo de reporte inválido (ventas | compras | inventario | caja)."),
        };
    }

    /// <summary>Exporta el reporte operativo completo (sin paginar). <paramref name="formato"/>: pdf | csv.</summary>
    [HttpGet("operativo/{tipo}/export")]
    [Authorize(Policy = "ver_reportes")]
    public async Task<IActionResult> ExportOperativo(
        string tipo,
        [FromQuery] string formato = "pdf",
        [FromQuery] string? desde = null, [FromQuery] string? hasta = null,
        [FromQuery] int? sucursalId = null, [FromQuery] string? metodoPago = null,
        [FromQuery] string? categoria = null, [FromQuery] int? operadorId = null,
        [FromQuery] string? tipoMov = null)
    {
        var f = ParseFiltros(desde, hasta, sucursalId, metodoPago, categoria, operadorId, tipoMov, 1, 20);
        var res = await opService.GetExportAsync(tipo, f);
        if (!res.IsSuccess) return ToHttpResponse(res);

        var data = res.Value!;
        return formato.ToLowerInvariant() == "csv"
            ? File(exporter.ToCsv(data), "text/csv; charset=utf-8", $"{data.FileBaseName}.csv")
            : File(exporter.ToPdf(data), "application/pdf", $"{data.FileBaseName}.pdf");
    }

    private static ReporteOperativoFiltros ParseFiltros(
        string? desde, string? hasta, int? sucursalId, string? metodoPago,
        string? categoria, int? operadorId, string? tipoMov, int page, int pageSize)
        => new()
        {
            Desde = DateOnly.TryParse(desde, out var d) ? d : null,
            Hasta = DateOnly.TryParse(hasta, out var h) ? h : null,
            SucursalId = sucursalId,
            MetodoPago = string.IsNullOrWhiteSpace(metodoPago) ? null : metodoPago,
            Categoria = string.IsNullOrWhiteSpace(categoria) ? null : categoria,
            OperadorId = operadorId,
            Tipo = string.IsNullOrWhiteSpace(tipoMov) ? null : tipoMov,
            Page = page < 1 ? 1 : page,
            PageSize = pageSize is < 1 or > 100 ? 20 : pageSize,
        };
}
