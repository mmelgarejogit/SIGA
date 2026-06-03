using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Stock;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/stock/ajustes")]
[Authorize]
public class AjustesManualController(IAjusteManualService svc) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] Guid? sucursalId = null, [FromQuery] string? estado = null)
        => ToHttpResponse(await svc.GetAllAsync(page, pageSize, ForcedSucursalId ?? sucursalId, estado));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => ToHttpResponse(await svc.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAjusteManualRequest request)
    {
        if (ForcedSucursalId.HasValue)
            request.SucursalId = ForcedSucursalId.Value;
        return ToHttpResponse(await svc.CreateAsync(request, CurrentUserId));
    }

    [HttpPost("{id:guid}/resolver")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Resolver(Guid id, [FromBody] ResolverAjusteRequest request)
        => ToHttpResponse(await svc.ResolverAsync(id, request, CurrentUserId));
}
