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
        [FromQuery] string? estado    = null,
        [FromQuery] string? tipo      = null,
        [FromQuery] string? fechaDesde = null,
        [FromQuery] string? fechaHasta = null,
        [FromQuery] int?    clienteId  = null,
        [FromQuery] int     page       = 1,
        [FromQuery] int     pageSize   = 10)
    {
        var result = await ventaService.GetVentasAsync(estado, tipo, fechaDesde, fechaHasta, clienteId, page, pageSize);
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
        var result = await ventaService.ConfirmarVentaAsync(id, CurrentUserId);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "registrar_venta")]
    public async Task<IActionResult> EliminarPresupuesto(int id)
    {
        var result = await ventaService.EliminarPresupuestoAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}/cancelar")]
    [Authorize(Policy = "registrar_venta")]
    public async Task<IActionResult> CancelarVenta(int id, [FromBody] CancelarVentaRequest request)
    {
        var result = await ventaService.CancelarVentaAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpGet("cobros-pendientes")]
    [Authorize(Policy = "ver_ventas")]
    public async Task<IActionResult> GetCobrosPendientes()
    {
        var result = await ventaService.GetCobrosPendientesAsync();
        return ToHttpResponse(result);
    }

    [HttpPost("cobros")]
    [Authorize(Policy = "registrar_venta")]
    public async Task<IActionResult> RegistrarCobro([FromBody] RegistrarCobroRequest request)
    {
        var result = await ventaService.RegistrarCobroAsync(request, CurrentUserId);
        return ToHttpResponse(result);
    }

    [HttpPost("{id:int}/comprobante")]
    [Authorize(Policy = "registrar_venta")]
    public async Task<IActionResult> EmitirComprobante(int id)
    {
        var result = await ventaService.EmitirComprobanteAsync(id, CurrentUserId);
        return ToHttpResponse(result);
    }

    [HttpPost("facturas")]
    [Authorize(Policy = "registrar_venta")]
    public async Task<IActionResult> EmitirFactura([FromBody] EmitirFacturaRequest request)
    {
        var result = await ventaService.EmitirFacturaAsync(request);
        return ToHttpResponse(result);
    }

    // ── Devoluciones ──────────────────────────────────────────────────────────────

    [HttpPost("{id:int}/devoluciones")]
    [Authorize(Policy = "registrar_venta")]
    public async Task<IActionResult> SolicitarDevolucion(int id, [FromBody] SolicitarDevolucionRequest request)
    {
        var result = await ventaService.SolicitarDevolucionAsync(id, request, CurrentUserId, CurrentUserName);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}/devoluciones")]
    [Authorize(Policy = "ver_ventas")]
    public async Task<IActionResult> GetDevoluciones(int id)
    {
        var result = await ventaService.GetDevolucionesAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost("devoluciones/{devolucionId:int}/gestionar")]
    [Authorize(Policy = "gestionar_ventas")]
    public async Task<IActionResult> GestionarDevolucion(int devolucionId, [FromBody] GestionarDevolucionRequest request)
    {
        var result = await ventaService.GestionarDevolucionAsync(devolucionId, request, CurrentUserId, CurrentUserName);
        return ToHttpResponse(result);
    }
}
