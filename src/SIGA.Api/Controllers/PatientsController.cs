using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Patients;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : BaseController
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null)
    {
        if (page < 1)    page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var result = await _patientService.GetAllAsync(page, pageSize, search, status);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _patientService.GetByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest request)
    {
        var result = await _patientService.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePatientRequest request)
    {
        var result = await _patientService.UpdateAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _patientService.DeleteAsync(id);
        return ToHttpResponse(result);
    }
}
