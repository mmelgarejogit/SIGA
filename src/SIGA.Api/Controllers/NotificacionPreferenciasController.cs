using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Notificaciones;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/notificaciones/preferencias")]
[Authorize]
public class NotificacionPreferenciasController(INotificacionPreferenciaService service) : BaseController
{
    /// <summary>Preferencias de notificación del usuario autenticado (self-service).</summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetPropia()
    {
        var result = await service.GetPropiaAsync();
        return ToHttpResponse(result);
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdatePropia([FromBody] UpdateNotificacionPreferenciaRequest request)
    {
        var result = await service.UpdatePropiaAsync(request);
        return ToHttpResponse(result);
    }

    /// <summary>Preferencias de una persona (paciente/cliente), gestionadas por staff.</summary>
    [HttpGet("persona/{personId:int}")]
    [Authorize(Policy = "gestionar_notificaciones")]
    public async Task<IActionResult> GetByPersona(int personId)
    {
        var result = await service.GetByPersonaAsync(personId);
        return ToHttpResponse(result);
    }

    [HttpPut("persona/{personId:int}")]
    [Authorize(Policy = "gestionar_notificaciones")]
    public async Task<IActionResult> UpdateByPersona(int personId, [FromBody] UpdateNotificacionPreferenciaRequest request)
    {
        var result = await service.UpdateByPersonaAsync(personId, request);
        return ToHttpResponse(result);
    }
}
