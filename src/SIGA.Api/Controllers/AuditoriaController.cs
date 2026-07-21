using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Auditoria;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/auditoria")]
[Authorize]
public class AuditoriaController(IAuditService audit) : BaseController
{
    [HttpGet]
    [Authorize(Policy = "ver_usuarios")]
    public async Task<IActionResult> Get([FromQuery] AuditoriaFiltros filtros)
        => ToHttpResponse(await audit.GetRegistrosAsync(filtros));

    [HttpGet("acciones")]
    [Authorize(Policy = "ver_usuarios")]
    public IActionResult Acciones() => Ok(audit.GetAcciones());
}
