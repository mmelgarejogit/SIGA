using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Compras;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/compras/facturas")]
[Authorize(Policy = "ver_inventario")]
public class FacturasCompraController(IFacturasCompraService service) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetFacturas(
        [FromQuery] int? proveedorId      = null,
        [FromQuery] string? condicionVenta = null,
        [FromQuery] string? estado         = null,
        [FromQuery] string? origen         = null,
        [FromQuery] string? fechaDesde     = null,
        [FromQuery] string? fechaHasta     = null,
        [FromQuery] string? search         = null,
        [FromQuery] int page               = 1,
        [FromQuery] int pageSize           = 20)
    {
        var result = await service.GetFacturasAsync(
            proveedorId, condicionVenta, estado, origen,
            fechaDesde, fechaHasta, search, page, pageSize);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetFactura(int id)
    {
        var result = await service.GetFacturaByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "gestionar_pedidos")]
    public async Task<IActionResult> RegistrarFacturaDirecta([FromBody] RegistrarFacturaDirectaRequest request)
    {
        var result = await service.RegistrarFacturaDirectaAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}/anular")]
    [Authorize(Policy = "gestionar_pedidos")]
    public async Task<IActionResult> AnularFactura(int id, [FromBody] AnularFacturaRequest request)
    {
        var result = await service.AnularFacturaAsync(id, request);
        return ToHttpResponse(result);
    }
}
