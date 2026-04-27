using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Clinica;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConsultasController : BaseController
{
    private readonly IConsultaClinicaService _service;

    public ConsultasController(IConsultaClinicaService service) => _service = service;

    [HttpGet]
    [Authorize(Policy = "ver_historia_clinica")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? patientId = null,
        [FromQuery] int? professionalId = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 500) pageSize = 10;

        var result = await _service.GetAllAsync(page, pageSize, search, patientId, professionalId);
        return ToHttpResponse(result);
    }

    [HttpGet("patient/{patientId:int}")]
    [Authorize(Policy = "ver_historia_clinica")]
    public async Task<IActionResult> GetByPatient(int patientId)
    {
        var result = await _service.GetByPatientAsync(patientId);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "ver_historia_clinica")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "ver_historia_clinica")]
    public async Task<IActionResult> Create([FromBody] CreateConsultaClinicaRequest request)
    {
        var result = await _service.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "ver_historia_clinica")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateConsultaClinicaRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "ver_historia_clinica")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost("{id:int}/receta")]
    [Authorize(Policy = "ver_historia_clinica")]
    public async Task<IActionResult> CreateOrUpdateReceta(int id, [FromBody] CreateRecetaRequest request)
    {
        var result = await _service.CreateOrUpdateRecetaAsync(id, request);
        return ToHttpResponse(result);
    }
}
