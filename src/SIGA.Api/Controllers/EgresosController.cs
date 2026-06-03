using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Egresos;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/egresos")]
[Authorize(Policy = "gestionar_egresos")]
public class EgresosController(IEgresoService egresoService) : BaseController
{
    private int CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    [HttpGet]
    [Authorize(Policy = "ver_egresos")]
    public async Task<IActionResult> GetEgresos(
        [FromQuery] string? tipo = null,
        [FromQuery] string? estado = null,
        [FromQuery] string? fechaDesde = null,
        [FromQuery] string? fechaHasta = null,
        [FromQuery] bool? soloVencidos = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] Guid? sucursalId = null)
    {
        var result = await egresoService.GetEgresosAsync(tipo, estado, fechaDesde, fechaHasta, soloVencidos, page, pageSize, sucursalId);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "ver_egresos")]
    public async Task<IActionResult> GetEgreso(int id)
    {
        var result = await egresoService.GetEgresoByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost("honorarios")]
    public async Task<IActionResult> CrearHonorario([FromBody] CrearHonorarioRequest request)
    {
        var result = await egresoService.CrearHonorarioAsync(request, CurrentUserId);
        return ToHttpResponse(result);
    }

    [HttpPost("salarios")]
    public async Task<IActionResult> CrearSalario([FromBody] CrearSalarioRequest request)
    {
        var result = await egresoService.CrearSalarioAsync(request, CurrentUserId);
        return ToHttpResponse(result);
    }

    [HttpPost("gastos")]
    public async Task<IActionResult> CrearGastoGeneral([FromBody] CrearGastoGeneralRequest request)
    {
        var result = await egresoService.CrearGastoGeneralAsync(request, CurrentUserId);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}/aprobar")]
    [Authorize(Policy = "aprobar_egresos")]
    public async Task<IActionResult> AprobarEgreso(int id)
    {
        var result = await egresoService.AprobarEgresoAsync(id, CurrentUserId);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}/rechazar")]
    [Authorize(Policy = "aprobar_egresos")]
    public async Task<IActionResult> RechazarEgreso(int id, [FromBody] RechazarEgresoRequest request)
    {
        var result = await egresoService.RechazarEgresoAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}/pago")]
    [Authorize(Policy = "pagar_egresos")]
    public async Task<IActionResult> RegistrarPago(int id, [FromBody] RegistrarPagoRequest request)
    {
        var result = await egresoService.RegistrarPagoAsync(id, request, CurrentUserId);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}/anular")]
    public async Task<IActionResult> AnularEgreso(int id, [FromBody] AnularEgresoRequest request)
    {
        var result = await egresoService.AnularEgresoAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpGet("categorias")]
    [Authorize(Policy = "ver_egresos")]
    public async Task<IActionResult> GetCategorias()
    {
        var result = await egresoService.GetCategoriasAsync();
        return ToHttpResponse(result);
    }

    [HttpPost("categorias")]
    public async Task<IActionResult> CrearCategoria([FromBody] CrearCategoriaGastoRequest request)
    {
        var result = await egresoService.CrearCategoriaAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("categorias/{id:int}")]
    public async Task<IActionResult> ActualizarCategoria(int id, [FromBody] ActualizarCategoriaGastoRequest request)
    {
        var result = await egresoService.ActualizarCategoriaAsync(id, request);
        return ToHttpResponse(result);
    }
}