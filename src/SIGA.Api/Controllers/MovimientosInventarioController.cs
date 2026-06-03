using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/stock/movimientos")]
[Authorize]
public class MovimientosInventarioController(IMovimientoInventarioService svc) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] Guid? sucursalId = null, [FromQuery] Guid? productoVarianteId = null,
        [FromQuery] string? tipo = null, [FromQuery] string? origen = null)
        => ToHttpResponse(await svc.GetAllAsync(page, pageSize, ForcedSucursalId ?? sucursalId, productoVarianteId, tipo, origen));

    [HttpGet("stock")]
    public async Task<IActionResult> GetStock(
        [FromQuery] Guid? sucursalId = null,
        [FromQuery] Guid? productoVarianteId = null,
        [FromQuery] bool? bajoStock = null)
        => ToHttpResponse(await svc.GetStockAsync(ForcedSucursalId ?? sucursalId, productoVarianteId, bajoStock));

    [HttpGet("stock/{varianteId:guid}/{sucursalId:guid}")]
    public async Task<IActionResult> GetStockActual(Guid varianteId, Guid sucursalId)
        => ToHttpResponse(await svc.GetStockActualAsync(varianteId, sucursalId));
}
