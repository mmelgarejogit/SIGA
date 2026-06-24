using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Ventas;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/laboratorio")]
[Authorize]
public class LaboratorioController(ILaboratorioService lab) : BaseController
{
    private int CurrentUserId =>
        int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    private string CurrentUserName =>
        User.FindFirst("name")?.Value ?? $"Usuario #{CurrentUserId}";

    [HttpGet("pedidos")]
    [Authorize(Policy = "ver_laboratorio")]
    public async Task<IActionResult> GetPedidos([FromQuery] string? estado = null)
        => ToHttpResponse(await lab.GetPedidosAsync(estado));

    [HttpPost("pedidos/{id:int}/gestionar")]
    [Authorize(Policy = "gestionar_laboratorio")]
    public async Task<IActionResult> Gestionar(int id, [FromBody] GestionarTrabajoPedidoRequest request)
        => ToHttpResponse(await lab.GestionarAprobacionAsync(id, request, CurrentUserId, CurrentUserName));

    [HttpPut("pedidos/{id:int}/enviar")]
    [Authorize(Policy = "gestionar_laboratorio")]
    public async Task<IActionResult> Enviar(int id)
        => ToHttpResponse(await lab.RegistrarEnvioAsync(id));

    [HttpPut("pedidos/{id:int}/recibir")]
    [Authorize(Policy = "gestionar_laboratorio")]
    public async Task<IActionResult> Recibir(int id)
        => ToHttpResponse(await lab.RegistrarRecepcionAsync(id));

    [HttpPost("pedidos/{id:int}/factura")]
    [Authorize(Policy = "gestionar_laboratorio")]
    public async Task<IActionResult> Factura(int id, [FromBody] EmitirFacturaLaboratorioRequest request)
        => ToHttpResponse(await lab.EmitirFacturaAsync(id, request, CurrentUserId));
}
