using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Compras;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/compras/pedidos")]
[Authorize(Policy = "gestionar_pedidos")]
public class ComprasController(IComprasService comprasService) : BaseController
{
    [HttpGet]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetPedidos(
        [FromQuery] int? proveedorId = null,
        [FromQuery] string? estado = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await comprasService.GetPedidosAsync(proveedorId, estado, page, pageSize);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetPedido(int id)
    {
        var result = await comprasService.GetPedidoByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost]
    public async Task<IActionResult> CrearPedido([FromBody] CrearPedidoRequest request)
    {
        var result = await comprasService.CrearPedidoAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> EditarPedido(int id, [FromBody] CrearPedidoRequest request)
    {
        var result = await comprasService.EditarPedidoAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}/confirmar")]
    public async Task<IActionResult> ConfirmarPedido(int id)
    {
        var result = await comprasService.ConfirmarPedidoAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost("{id:int}/factura")]
    public async Task<IActionResult> RegistrarFactura(int id, [FromBody] RegistrarFacturaPedidoRequest request)
    {
        var result = await comprasService.RegistrarFacturaAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpPost("{id:int}/devolucion")]
    public async Task<IActionResult> RegistrarDevolucion(int id, [FromBody] RegistrarDevolucionRequest request)
    {
        var result = await comprasService.RegistrarDevolucionAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}/cancelar")]
    public async Task<IActionResult> CancelarPedido(int id)
    {
        var result = await comprasService.CancelarPedidoAsync(id);
        return ToHttpResponse(result);
    }
}
