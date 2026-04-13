using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Professionals;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProfessionalsController : BaseController
{
    private readonly IProfessionalService _professionalService;

    public ProfessionalsController(IProfessionalService professionalService)
    {
        _professionalService = professionalService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _professionalService.GetAllAsync();
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _professionalService.GetByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProfessionalRequest request)
    {
        var result = await _professionalService.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProfessionalRequest request)
    {
        var result = await _professionalService.UpdateAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _professionalService.DeleteAsync(id);
        return ToHttpResponse(result);
    }
}
