using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Productos;
using SIGA.Application.Interfaces;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/productos")]
[Authorize]
public class ProductosController(IProductoService svc) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null, [FromQuery] int? categoriaId = null,
        [FromQuery] bool? isActive = null)
        => ToHttpResponse(await svc.GetAllAsync(page, pageSize, search, categoriaId, isActive));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => ToHttpResponse(await svc.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductoRequest request)
        => ToHttpResponse(await svc.CreateAsync(request));

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductoRequest request)
        => ToHttpResponse(await svc.UpdateAsync(id, request));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Deactivate(int id)
        => ToHttpResponse(await svc.DeactivateAsync(id));

    // ── Variantes ─────────────────────────────────────────────────────────────

    [HttpGet("{productoId:int}/variantes")]
    public async Task<IActionResult> GetVariantes(int productoId)
        => ToHttpResponse(await svc.GetVariantesAsync(productoId));

    [HttpGet("variantes/{id:guid}")]
    public async Task<IActionResult> GetVariante(Guid id)
        => ToHttpResponse(await svc.GetVarianteByIdAsync(id));

    [HttpPost("variantes")]
    public async Task<IActionResult> CreateVariante([FromBody] CreateProductoVarianteRequest request)
        => ToHttpResponse(await svc.CreateVarianteAsync(request));

    [HttpPut("variantes/{id:guid}")]
    public async Task<IActionResult> UpdateVariante(Guid id, [FromBody] UpdateProductoVarianteRequest request)
        => ToHttpResponse(await svc.UpdateVarianteAsync(id, request));

    [HttpDelete("variantes/{id:guid}")]
    public async Task<IActionResult> DeactivateVariante(Guid id)
        => ToHttpResponse(await svc.DeactivateVarianteAsync(id));

    [HttpPost("variantes/{id:guid}/imagen")]
    public async Task<IActionResult> UploadImagen(Guid id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "El archivo es requerido." });
        return ToHttpResponse(await svc.UploadVarianteImagenAsync(id, file.OpenReadStream(), file.FileName));
    }

    // ── Categorías ────────────────────────────────────────────────────────────

    [HttpGet("categorias")]
    public async Task<IActionResult> GetCategorias()
        => ToHttpResponse(await svc.GetCategoriasAsync());

    [HttpPost("categorias")]
    public async Task<IActionResult> CreateCategoria([FromBody] CreateCategoriaProductoRequest request)
        => ToHttpResponse(await svc.CreateCategoriaAsync(request));

    [HttpPut("categorias/{id:int}")]
    public async Task<IActionResult> UpdateCategoria(int id, [FromBody] UpdateCategoriaProductoRequest request)
        => ToHttpResponse(await svc.UpdateCategoriaAsync(id, request));

    [HttpDelete("categorias/{id:int}")]
    public async Task<IActionResult> DeactivateCategoria(int id)
        => ToHttpResponse(await svc.DeactivateCategoriaAsync(id));
}
