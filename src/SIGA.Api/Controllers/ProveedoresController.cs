using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Productos;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProveedoresController(IProveedorService proveedorService) : BaseController
{
    [HttpGet]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await proveedorService.GetAllAsync(page, pageSize, search, isActive);
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
}
