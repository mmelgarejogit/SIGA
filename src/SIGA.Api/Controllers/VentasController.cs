using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Ventas;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/ventas")]
[Authorize]
public class VentasController(IVentaService ventaService) : BaseController
{
    private int CurrentUserId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    private string CurrentUserName =>
        User.FindFirst("name")?.Value ?? $"Usuario #{CurrentUserId}";

    [HttpGet]
    [Authorize(Policy = "ver_ventas")]
    public async Task<IActionResult> GetVentas(
        [FromQuery] string? estado = null,
        [FromQuery] string? fechaDesde = null,
        [FromQuery] string? fechaHasta = null,
        [FromQuery] int? patientId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await ventaService.GetVentasAsync(estado, fechaDesde, fechaHasta, patientId, page, pageSize);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "ver_ventas")]
    public async Task<IActionResult> GetVenta(int id)
    {
        var result = await ventaService.GetVentaByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "registrar_venta")]
    public async Task<IActionResult> CrearVenta([FromBody] CrearVentaRequest request)
    {
        var result = await ventaService.CrearVentaAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}/confirmar")]
    [Authorize(Policy = "registrar_venta")]
    public async Task<IActionResult> ConfirmarVenta(int id)
    {
        var result = await ventaService.ConfirmarVentaAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost("cobros")]
    [Authorize(Policy = "registrar_venta")]
    public async Task<IActionResult> RegistrarCobro([FromBody] RegistrarCobroRequest request)
    {
        var result = await ventaService.RegistrarCobroAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPost("facturas")]
    [Authorize(Policy = "registrar_venta")]
    public async Task<IActionResult> EmitirFactura([FromBody] EmitirFacturaRequest request)
    {
        var result = await ventaService.EmitirFacturaAsync(request);
        return ToHttpResponse(result);
    }

    // ── Anulación con aprobación ──────────────────────────────────────────────────

    [HttpPost("{id:int}/solicitar-anulacion")]
    [Authorize(Policy = "registrar_venta")]
    public async Task<IActionResult> SolicitarAnulacion(int id, [FromBody] SolicitarAnulacionRequest request)
    {
        request.VentaId = id;
        var result = await ventaService.SolicitarAnulacionAsync(CurrentUserId, CurrentUserName, request);
        return ToHttpResponse(result);
    }

    [HttpGet("anulaciones")]
    [Authorize(Policy = "gestionar_ventas")]
    public async Task<IActionResult> GetSolicitudesAnulacion([FromQuery] string? estado = null)
    {
        var result = await ventaService.GetSolicitudesAnulacionAsync(estado);
        return ToHttpResponse(result);
    }

    [HttpPost("anulaciones/{solicitudId:int}/gestionar")]
    [Authorize(Policy = "gestionar_ventas")]
    public async Task<IActionResult> GestionarAnulacion(
        int solicitudId, [FromBody] GestionarAnulacionVentaRequest request)
    {
        var result = await ventaService.GestionarAnulacionAsync(
            solicitudId, CurrentUserId, CurrentUserName, request);
        return ToHttpResponse(result);
    }
}
