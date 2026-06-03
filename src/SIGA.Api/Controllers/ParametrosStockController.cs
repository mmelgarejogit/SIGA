using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Stock;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/stock/parametros")]
[Authorize]
public class ParametrosStockController(IParametroStockService svc) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? sucursalId = null,
        [FromQuery] Guid? productoVarianteId = null)
        => ToHttpResponse(await svc.GetAllAsync(sucursalId, productoVarianteId));

    [HttpPut]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Upsert([FromBody] UpsertParametroStockRequest request)
        => ToHttpResponse(await svc.UpsertAsync(request));

    [HttpDelete]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Delete(
        [FromQuery] Guid productoVarianteId,
        [FromQuery] Guid sucursalId)
        => ToHttpResponse(await svc.DeleteAsync(productoVarianteId, sucursalId));
}
