using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Stock;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/tipos-ajuste")]
[Authorize]
public class TiposAjusteController(ITipoAjusteService svc) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? impacto = null, [FromQuery] bool? activo = null)
        => ToHttpResponse(await svc.GetAllAsync(impacto, activo));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
        => ToHttpResponse(await svc.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Policy = "gestionar_configuracion")]
    public async Task<IActionResult> Create([FromBody] CreateTipoAjusteRequest request)
        => ToHttpResponse(await svc.CreateAsync(request));

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "gestionar_configuracion")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTipoAjusteRequest request)
        => ToHttpResponse(await svc.UpdateAsync(id, request));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "gestionar_configuracion")]
    public async Task<IActionResult> Deactivate(Guid id)
        => ToHttpResponse(await svc.DeactivateAsync(id));
}
