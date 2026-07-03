using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "ver_notificaciones")]
public class NotificacionesController : BaseController
{
    private readonly INotificacionInternaService _notificacionService;

    public NotificacionesController(INotificacionInternaService notificacionService)
    {
        _notificacionService = notificacionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMisNotificaciones(
        [FromQuery] bool? soloNoLeidas, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _notificacionService.GetMisNotificacionesAsync(soloNoLeidas, page, pageSize);
        return ToHttpResponse(result);
    }

    [HttpGet("contador")]
    public async Task<IActionResult> GetContador()
    {
        var result = await _notificacionService.GetContadorNoLeidasAsync();
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}/leer")]
    public async Task<IActionResult> MarcarLeida(int id)
    {
        var result = await _notificacionService.MarcarLeidaAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPut("leer-todas")]
    public async Task<IActionResult> MarcarTodasLeidas()
    {
        var result = await _notificacionService.MarcarTodasLeidasAsync();
        return ToHttpResponse(result);
    }
}
