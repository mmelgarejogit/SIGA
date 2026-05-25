using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Compras;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/compras/recepciones")]
[Authorize(Policy = "ver_inventario")]
public class RecepcionesController(IRecepcionesService service) : BaseController
{
    private int CurrentUserId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    [HttpGet]
    public async Task<IActionResult> GetRecepciones(
        [FromQuery] int? proveedorId = null,
        [FromQuery] string? estadoOC = null,
        [FromQuery] string? fechaDesde = null,
        [FromQuery] string? fechaHasta = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await service.GetRecepcionesAsync(
            proveedorId, estadoOC, fechaDesde, fechaHasta, page, pageSize);
        return ToHttpResponse(result);
    }

    [HttpGet("facturas-disponibles")]
    public async Task<IActionResult> GetFacturasDisponibles()
    {
        var result = await service.GetFacturasDisponiblesAsync();
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "gestionar_pedidos")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarRecepcionRequest request)
    {
        var result = await service.RegistrarRecepcionAsync(CurrentUserId, request);
        return ToHttpResponse(result);
    }
}
