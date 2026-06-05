using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Ubicacion;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/ubicaciones")]
[Authorize]
public class UbicacionesController(IUbicacionService service) : BaseController
{
    [HttpGet("departamentos")]
    public async Task<IActionResult> GetDepartamentos([FromQuery] bool? isActive)
        => ToHttpResponse(await service.GetDepartamentosAsync(isActive));

    [HttpPost("departamentos")]
    [Authorize(Policy = "gestionar_configuracion")]
    public async Task<IActionResult> CreateDepartamento([FromBody] CreateDepartamentoRequest request)
        => ToHttpResponse(await service.CreateDepartamentoAsync(request));

    [HttpPut("departamentos/{id:int}")]
    [Authorize(Policy = "gestionar_configuracion")]
    public async Task<IActionResult> UpdateDepartamento(int id, [FromBody] UpdateDepartamentoRequest request)
        => ToHttpResponse(await service.UpdateDepartamentoAsync(id, request));

    [HttpGet("ciudades")]
    public async Task<IActionResult> GetCiudades([FromQuery] int? departamentoId, [FromQuery] bool? isActive)
        => ToHttpResponse(await service.GetCiudadesAsync(departamentoId, isActive));

    [HttpPost("ciudades")]
    [Authorize(Policy = "gestionar_configuracion")]
    public async Task<IActionResult> CreateCiudad([FromBody] CreateCiudadRequest request)
        => ToHttpResponse(await service.CreateCiudadAsync(request));

    [HttpPut("ciudades/{id:int}")]
    [Authorize(Policy = "gestionar_configuracion")]
    public async Task<IActionResult> UpdateCiudad(int id, [FromBody] UpdateCiudadRequest request)
        => ToHttpResponse(await service.UpdateCiudadAsync(id, request));
}
