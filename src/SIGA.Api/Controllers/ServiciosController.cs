using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Ventas;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/servicios")]
[Authorize]
public class ServiciosController(IServicioService service) : BaseController
{
    [HttpGet]
    [Authorize(Policy = "ver_ventas")]
    public async Task<IActionResult> GetAll()
    {
        var result = await service.GetAllAsync();
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "gestionar_ventas")]
    public async Task<IActionResult> Create([FromBody] CreateServicioRequest request)
    {
        var result = await service.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "gestionar_ventas")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateServicioRequest request)
    {
        var result = await service.UpdateAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "gestionar_ventas")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await service.DeactivateAsync(id);
        return ToHttpResponse(result);
    }

    // ── Tarifas (precios por profesional / especialidad) ──────────────────────

    [HttpPost("{id:int}/tarifas")]
    [Authorize(Policy = "gestionar_ventas")]
    public async Task<IActionResult> AddTarifa(int id, [FromBody] CreateServicioTarifaRequest request)
    {
        var result = await service.AddTarifaAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("tarifas/{tarifaId:int}")]
    [Authorize(Policy = "gestionar_ventas")]
    public async Task<IActionResult> RemoveTarifa(int tarifaId)
    {
        var result = await service.RemoveTarifaAsync(tarifaId);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}/precio")]
    [Authorize(Policy = "ver_ventas")]
    public async Task<IActionResult> ResolvePrecio(int id, [FromQuery] int? professionalId)
    {
        var result = await service.ResolvePrecioAsync(id, professionalId);
        return ToHttpResponse(result);
    }
}
