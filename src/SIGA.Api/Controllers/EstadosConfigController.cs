using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Estados;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/estados-config")]
[Authorize]
public class EstadosConfigController(IEstadoConfigService service) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetByEntidad([FromQuery] string? entidad)
    {
        var result = await service.GetByEntidadAsync(entidad);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "gestionar_configuracion")]
    public async Task<IActionResult> Create([FromBody] CreateEstadoConfigRequest request)
    {
        var result = await service.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "gestionar_configuracion")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEstadoConfigRequest request)
    {
        var result = await service.UpdateAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "gestionar_configuracion")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await service.DeleteAsync(id);
        return ToHttpResponse(result);
    }
}
