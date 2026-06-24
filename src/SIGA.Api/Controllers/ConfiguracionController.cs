using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Configuracion;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConfiguracionController : BaseController
{
    private readonly IConfiguracionNegocioService _service;

    public ConfiguracionController(IConfiguracionNegocioService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _service.GetAsync();
        return ToHttpResponse(result);
    }

    [HttpPut]
    [Authorize(Policy = "gestionar_configuracion")]
    public async Task<IActionResult> Update([FromBody] UpdateConfiguracionNegocioRequest request)
    {
        var result = await _service.UpdateAsync(request);
        return ToHttpResponse(result);
    }
}
