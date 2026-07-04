using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Sucursales;
using SIGA.Application.Interfaces;
using System.Security.Claims;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "transferir_stock")]
public class TransferenciasController : BaseController
{
    private readonly ITransferenciaStockService _service;

    public TransferenciasController(ITransferenciaStockService service)
    {
        _service = service;
    }

    private int CurrentUserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
    private string CurrentUserName => User.FindFirst("name")?.Value ?? "";

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? estado)
    {
        var result = await _service.GetAllAsync(estado);
        return ToHttpResponse(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransferenciaRequest request)
    {
        var result = await _service.CreateAsync(request, CurrentUserId, CurrentUserName);
        return ToHttpResponse(result);
    }

    [HttpPost("{id:int}/gestionar")]
    public async Task<IActionResult> Gestionar(int id, [FromBody] GestionarTransferenciaRequest request)
    {
        var result = await _service.GestionarAsync(id, request, CurrentUserName);
        return ToHttpResponse(result);
    }
}
