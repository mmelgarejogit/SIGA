using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Productos;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MarcasController(IMarcaService marcaService) : BaseController
{
    // ── Marcas ─────────────────────────────────────────────────────────────────

    [HttpGet]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetMarcas()
    {
        var result = await marcaService.GetMarcasAsync();
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> CreateMarca([FromBody] CreateMarcaRequest request)
    {
        var result = await marcaService.CreateMarcaAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> UpdateMarca(int id, [FromBody] UpdateMarcaRequest request)
    {
        var result = await marcaService.UpdateMarcaAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> DeactivateMarca(int id)
    {
        var result = await marcaService.DeactivateMarcaAsync(id);
        return ToHttpResponse(result);
    }

    // ── Modelos ────────────────────────────────────────────────────────────────

    [HttpGet("modelos")]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetModelos([FromQuery] int? marcaId = null)
    {
        var result = await marcaService.GetModelosAsync(marcaId);
        return ToHttpResponse(result);
    }

    [HttpPost("modelos")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> CreateModelo([FromBody] CreateModeloRequest request)
    {
        var result = await marcaService.CreateModeloAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("modelos/{id:int}")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> UpdateModelo(int id, [FromBody] UpdateModeloRequest request)
    {
        var result = await marcaService.UpdateModeloAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("modelos/{id:int}")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> DeactivateModelo(int id)
    {
        var result = await marcaService.DeactivateModeloAsync(id);
        return ToHttpResponse(result);
    }
}
