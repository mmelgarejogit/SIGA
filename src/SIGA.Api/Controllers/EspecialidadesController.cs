using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Especialidades;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EspecialidadesController : BaseController
{
    private readonly IEspecialidadService _especialidadService;

    public EspecialidadesController(IEspecialidadService especialidadService)
    {
        _especialidadService = especialidadService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _especialidadService.GetAllAsync();
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _especialidadService.GetByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "gestionar_especialidades")]
    public async Task<IActionResult> Create([FromBody] CreateEspecialidadRequest request)
    {
        var result = await _especialidadService.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "gestionar_especialidades")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateEspecialidadRequest request)
    {
        var result = await _especialidadService.UpdateAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "gestionar_especialidades")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _especialidadService.DeleteAsync(id);
        return ToHttpResponse(result);
    }
}
