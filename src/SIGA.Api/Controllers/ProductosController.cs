using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController(IProductoService productoService) : BaseController
{
    [HttpGet]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? categoria = null,
        [FromQuery] bool? bajoStock = null)
    {
        var result = await productoService.GetAllAsync(page, pageSize, search, categoria, bajoStock);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await productoService.GetByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Create([FromBody] CreateProductoRequest request)
    {
        var result = await productoService.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductoRequest request)
    {
        var result = await productoService.UpdateAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await productoService.DeactivateAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost("{id:int}/movimientos")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> RegistrarMovimiento(int id, [FromBody] CreateMovimientoStockRequest request)
    {
        var result = await productoService.RegistrarMovimientoAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}/movimientos")]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetMovimientos(int id)
    {
        var result = await productoService.GetMovimientosAsync(id);
        return ToHttpResponse(result);
    }

    [HttpGet("movimientos")]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetAllMovimientos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? tipo = null)
    {
        var result = await productoService.GetAllMovimientosAsync(page, pageSize, tipo);
        return ToHttpResponse(result);
    }
}
