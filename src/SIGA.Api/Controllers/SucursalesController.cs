using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Sucursales;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SucursalesController : BaseController
{
    private readonly ISucursalService _sucursalService;

    public SucursalesController(ISucursalService sucursalService)
    {
        _sucursalService = sucursalService;
    }

    // Lectura abierta a cualquier usuario autenticado: la lista de sucursales no es sensible
    // y el paciente la necesita para reservar turnos.
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool soloActivas = false)
    {
        var result = await _sucursalService.GetAllAsync(soloActivas);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _sucursalService.GetByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "gestionar_sucursales")]
    public async Task<IActionResult> Create([FromBody] CreateSucursalRequest request)
    {
        var result = await _sucursalService.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "gestionar_sucursales")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSucursalRequest request)
    {
        var result = await _sucursalService.UpdateAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "gestionar_sucursales")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _sucursalService.DeleteAsync(id);
        return ToHttpResponse(result);
    }
}
