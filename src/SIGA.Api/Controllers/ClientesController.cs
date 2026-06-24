using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Clientes;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ClientesController : BaseController
{
    private readonly IClienteService _clienteService;

    public ClientesController(IClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    [Authorize(Policy = "ver_clientes")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] string? tipo = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 500) pageSize = 10;

        var result = await _clienteService.GetAllAsync(page, pageSize, search, status, tipo);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "ver_clientes")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _clienteService.GetByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpGet("buscar-persona")]
    [Authorize(Policy = "crear_cliente")]
    public async Task<IActionResult> BuscarPersona([FromQuery] string ci)
    {
        var result = await _clienteService.BuscarPersonaPorCiAsync(ci);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "crear_cliente")]
    public async Task<IActionResult> Create([FromBody] CreateClienteRequest request)
    {
        var result = await _clienteService.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "editar_cliente")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClienteRequest request)
    {
        var result = await _clienteService.UpdateAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "desactivar_cliente")]
    public async Task<IActionResult> Desactivar(int id)
    {
        var result = await _clienteService.DesactivarAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost("{id:int}/activar")]
    [Authorize(Policy = "desactivar_cliente")]
    public async Task<IActionResult> Activar(int id)
    {
        var result = await _clienteService.ActivarAsync(id);
        return ToHttpResponse(result);
    }
}
