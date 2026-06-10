using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Ventas;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/timbrados")]
[Authorize]
public class TimbradosController(ITimbradoService service) : BaseController
{
    [HttpGet]
    [Authorize(Policy = "ver_ventas")]
    public async Task<IActionResult> GetAll()
    {
        var result = await service.GetAllAsync();
        return ToHttpResponse(result);
    }

    [HttpGet("activos")]
    [Authorize(Policy = "ver_ventas")]
    public async Task<IActionResult> GetActivos()
    {
        var result = await service.GetActivosAsync();
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "ver_ventas")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await service.GetByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "gestionar_ventas")]
    public async Task<IActionResult> Create([FromBody] CreateTimbradoRequest request)
    {
        var result = await service.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "gestionar_ventas")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTimbradoRequest request)
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
}