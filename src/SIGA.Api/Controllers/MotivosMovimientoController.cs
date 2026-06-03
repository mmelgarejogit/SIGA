using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Productos;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/motivos-movimiento")]
[Authorize]
public class MotivosMovimientoController(IMotivoMovimientoService service) : BaseController
{
    [HttpGet]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetAll([FromQuery] string? tipo = null)
    {
        var result = await service.GetAllAsync(tipo);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Create([FromBody] CreateMotivoMovimientoRequest request)
    {
        var result = await service.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMotivoMovimientoRequest request)
    {
        var result = await service.UpdateAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await service.DeactivateAsync(id);
        return ToHttpResponse(result);
    }
}
