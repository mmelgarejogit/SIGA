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
    private readonly IRecetaPdfGenerator _pdfGenerator;

    public ConsultasController(IConsultaClinicaService service, IRecetaPdfGenerator pdfGenerator)
    {
        _service = service;
        _pdfGenerator = pdfGenerator;
    }

    private int? CallerProfessionalId =>
        User.FindFirst("professional_id") is { } c && int.TryParse(c.Value, out var id) ? id : null;

    [HttpGet]
    [Authorize(Policy = "ver_consultas")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? patientId = null,
        [FromQuery] int? professionalId = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 500) pageSize = 10;

        var callerProfId = CallerProfessionalId;
        if (callerProfId.HasValue)
            professionalId = callerProfId.Value;

        var result = await _service.GetAllAsync(page, pageSize, search, patientId, professionalId);
        return ToHttpResponse(result);
    }

    [HttpGet("patient/{patientId:int}")]
    [Authorize(Policy = "ver_consultas")]
    public async Task<IActionResult> GetByPatient(int patientId)
    {
        var result = await _service.GetByPatientAsync(patientId);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "ver_consultas")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpGet("profesional/stats")]
    [Authorize(Policy = "ver_consultas")]
    public async Task<IActionResult> GetProfessionalStats()
    {
        var profId = CallerProfessionalId;
        if (!profId.HasValue)
            return Forbid();

        var result = await _service.GetProfessionalStatsAsync(profId.Value);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "registrar_consulta")]
    public async Task<IActionResult> Create([FromBody] CreateConsultaClinicaRequest request)
    {
        var callerProfId = CallerProfessionalId;
        if (callerProfId.HasValue)
            request.ProfessionalId = callerProfId.Value;

        var result = await _service.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "editar_consulta")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateConsultaClinicaRequest request)
    {
        var callerProfId = CallerProfessionalId;
        if (callerProfId.HasValue)
            request.ProfessionalId = callerProfId.Value;

        var result = await _service.UpdateAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "eliminar_consulta")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost("{id:int}/receta")]
    [Authorize(Policy = "editar_consulta")]
    public async Task<IActionResult> CreateOrUpdateReceta(int id, [FromBody] CreateRecetaRequest request)
    {
        var result = await _service.CreateOrUpdateRecetaAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}/receta/pdf")]
    [Authorize(Policy = "ver_consultas")]
    public async Task<IActionResult> GetRecetaPdf(int id)
    {
        var result = await _service.GetByIdAsync(id);
        if (!result.IsSuccess)
            return ToHttpResponse(result);

        var consulta = result.Value!;
        if (consulta.Receta is null)
            return NotFound(new { message = "Esta consulta no tiene receta." });

        var pdf = _pdfGenerator.Generate(consulta);
        var filename = $"receta_{consulta.PatientLastName}_{consulta.Receta.FechaEmision:yyyyMMdd}.pdf";
        return File(pdf, "application/pdf", filename);
    }
}
