using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/stock/lotes")]
[Authorize(Policy = "ver_inventario")]
public class StockLotesController(IStockLoteService service) : BaseController
{
    private int CurrentUserId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    private string CurrentUserName =>
        User.FindFirst("name")?.Value ?? $"Usuario #{CurrentUserId}";

    [HttpGet]
    public async Task<IActionResult> GetLotes(
        [FromQuery] int? productoId = null,
        [FromQuery] bool? vencidos = null)
    {
        var result = await service.GetLotesAsync(productoId, vencidos);
        return ToHttpResponse(result);
    }

    [HttpPost("conteo")]
    public async Task<IActionResult> RegistrarConteo([FromBody] RegistrarConteoRequest request)
    {
        var result = await service.RegistrarConteoAsync(CurrentUserId, CurrentUserName, request);
        return ToHttpResponse(result);
    }

    [HttpGet("conteos")]
    public async Task<IActionResult> GetConteos([FromQuery] string? estado = null)
    {
        var result = await service.GetConteosAsync(estado);
        return ToHttpResponse(result);
    }

    [HttpGet("conteos/{id:int}")]
    public async Task<IActionResult> GetConteoById(int id)
    {
        var result = await service.GetConteoByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost("conteos/{id:int}/gestionar")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> GestionarConteo(int id, [FromBody] GestionarConteoRequest request)
    {
        var result = await service.GestionarConteoAsync(id, CurrentUserId, CurrentUserName, request);
        return ToHttpResponse(result);
    }
}
