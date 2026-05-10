using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProveedoresController(IProveedorService proveedorService) : BaseController
{
    [HttpGet]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetAll([FromQuery] string? search = null)
    {
        var result = await proveedorService.GetAllAsync(search);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "gestionar_pedidos")]
    public async Task<IActionResult> Create([FromBody] CreateProveedorRequest request)
    {
        var result = await proveedorService.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "gestionar_pedidos")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProveedorRequest request)
    {
        var result = await proveedorService.UpdateAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "gestionar_pedidos")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await proveedorService.DeactivateAsync(id);
        return ToHttpResponse(result);
    }

    [HttpGet("pedidos")]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetPedidos(
        [FromQuery] int? proveedorId = null,
        [FromQuery] string? estado = null)
    {
        var result = await proveedorService.GetPedidosAsync(proveedorId, estado);
        return ToHttpResponse(result);
    }

    [HttpPost("pedidos")]
    [Authorize(Policy = "gestionar_pedidos")]
    public async Task<IActionResult> CreatePedido([FromBody] CreatePedidoProveedorRequest request)
    {
        var result = await proveedorService.CreatePedidoAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("pedidos/{id:int}/estado")]
    [Authorize(Policy = "gestionar_pedidos")]
    public async Task<IActionResult> UpdatePedidoEstado(int id, [FromBody] UpdatePedidoEstadoBody body)
    {
        var result = await proveedorService.UpdatePedidoEstadoAsync(id, body.Estado);
        return ToHttpResponse(result);
    }

    [HttpDelete("pedidos/{id:int}")]
    [Authorize(Policy = "gestionar_pedidos")]
    public async Task<IActionResult> CancelPedido(int id)
    {
        var result = await proveedorService.CancelPedidoAsync(id);
        return ToHttpResponse(result);
    }
}

public record UpdatePedidoEstadoBody(string Estado);
