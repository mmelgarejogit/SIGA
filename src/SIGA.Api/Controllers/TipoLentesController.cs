using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/tipos-lente")]
[Authorize]
public class TipoLentesController(ITipoLenteService service) : BaseController
{
    [HttpGet]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetAll()
    {
        var result = await service.GetAllAsync();
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Create([FromBody] CreateTipoLenteRequest request)
    {
        var result = await service.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTipoLenteRequest request)
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
