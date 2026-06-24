using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Empleados;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/empleados")]
[Authorize(Policy = "ver_empleados")]
public class EmpleadosController(IEmpleadoService empleadoService) : ControllerBase
{
    private IActionResult ToResponse<T>(Application.Common.Result<T> result)
    {
        if (result.IsSuccess) return Ok(result.Value);
        return result.ErrorType switch
        {
            Application.Common.ErrorType.NotFound   => NotFound(new { message = result.Error }),
            Application.Common.ErrorType.Conflict   => Conflict(new { message = result.Error }),
            Application.Common.ErrorType.Validation => BadRequest(new { message = result.Error }),
            _                                       => StatusCode(500, new { message = result.Error }),
        };
    }

    // ── Empleados ─────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? soloActivos)
    {
        var result = await empleadoService.GetAllAsync(soloActivos);
        return ToResponse(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await empleadoService.GetByIdAsync(id);
        return ToResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "gestionar_empleados")]
    public async Task<IActionResult> Crear([FromBody] CrearEmpleadoRequest request)
    {
        var result = await empleadoService.CrearAsync(request);
        return ToResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "gestionar_empleados")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarEmpleadoRequest request)
    {
        var result = await empleadoService.ActualizarAsync(id, request);
        return ToResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "gestionar_empleados")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var result = await empleadoService.DesactivarAsync(id);
        return ToResponse(result);
    }

    // ── Cargos ────────────────────────────────────────────────────────────────

    [HttpGet("cargos")]
    public async Task<IActionResult> GetCargos()
    {
        var result = await empleadoService.GetCargosAsync();
        return ToResponse(result);
    }

    [HttpPost("cargos")]
    [Authorize(Policy = "gestionar_empleados")]
    public async Task<IActionResult> CrearCargo([FromBody] CrearCargoEmpleadoRequest request)
    {
        var result = await empleadoService.CrearCargoAsync(request);
        return ToResponse(result);
    }

    [HttpPut("cargos/{id:int}")]
    [Authorize(Policy = "gestionar_empleados")]
    public async Task<IActionResult> ActualizarCargo(int id, [FromBody] ActualizarCargoEmpleadoRequest request)
    {
        var result = await empleadoService.ActualizarCargoAsync(id, request);
        return ToResponse(result);
    }
}
