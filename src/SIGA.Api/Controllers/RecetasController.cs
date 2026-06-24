using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Clinica;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/recetas")]
[Authorize]
public class RecetasController(IRecetaService recetaService) : BaseController
{
    /// <summary>Recetas de un cliente (clínicas y cargadas a mano), para el flujo de venta a pedido.</summary>
    [HttpGet]
    [Authorize(Policy = "ver_ventas")]
    public async Task<IActionResult> GetByCliente([FromQuery] int clienteId)
    {
        var result = await recetaService.GetByClienteAsync(clienteId);
        return ToHttpResponse(result);
    }

    /// <summary>Carga una receta externa (sin consulta) vinculada a un cliente.</summary>
    [HttpPost]
    [Authorize(Policy = "registrar_venta")]
    public async Task<IActionResult> CreateManual([FromBody] CreateRecetaManualRequest request)
    {
        var result = await recetaService.CreateManualAsync(request);
        return ToHttpResponse(result);
    }
}
