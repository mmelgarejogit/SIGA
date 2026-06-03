using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Stock;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/stock/fisico")]
[Authorize]
public class InventarioFisicoController(IInventarioFisicoService svc) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] Guid? sucursalId = null, [FromQuery] string? estado = null)
        => ToHttpResponse(await svc.GetAllAsync(page, pageSize, ForcedSucursalId ?? sucursalId, estado));

    // Endpoint Admin: devuelve snapshot de cantidad_sistema
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> GetById(Guid id)
        => ToHttpResponse(await svc.GetByIdAsync(id, includeSnapshot: true));

    // Endpoint Encargado: oculta cantidad_sistema (conteo ciego)
    [HttpGet("{id:guid}/conteo")]
    public async Task<IActionResult> GetConteo(Guid id)
        => ToHttpResponse(await svc.GetByIdAsync(id, includeSnapshot: false));

    [HttpPost]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Create([FromBody] CreateInventarioFisicoRequest request)
        => ToHttpResponse(await svc.CreateAsync(request, CurrentUserId));

    [HttpPost("{id:guid}/iniciar")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> IniciarConteo(Guid id)
        => ToHttpResponse(await svc.IniciarConteoAsync(id, CurrentUserId));

    [HttpPut("{id:guid}/conteos")]
    public async Task<IActionResult> GuardarConteos(Guid id, [FromBody] GuardarConteosRequest request)
        => ToHttpResponse(await svc.GuardarConteosAsync(id, request, CurrentUserId));

    [HttpPost("{id:guid}/cerrar")]
    public async Task<IActionResult> Cerrar(Guid id)
        => ToHttpResponse(await svc.CerrarAsync(id, CurrentUserId));

    [HttpPost("{id:guid}/aprobar")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Aprobar(Guid id)
        => ToHttpResponse(await svc.AprobarAsync(id, CurrentUserId));

    [HttpPost("{id:guid}/cancelar")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Cancelar(Guid id)
        => ToHttpResponse(await svc.CancelarAsync(id, CurrentUserId));
}
