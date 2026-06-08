using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;
using SIGA.Application.Common;

namespace SIGA.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosController(IProductoService productoService, IMovimientoStockPdfGenerator pdfGenerator) : BaseController
{
    [HttpGet]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? categoria = null,
        [FromQuery] bool? bajoStock = null,
        [FromQuery] string? tipoCategoria = null)
    {
        var result = await productoService.GetAllAsync(page, pageSize, search, categoria, bajoStock, tipoCategoria);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await productoService.GetByIdAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Create([FromBody] CreateProductoRequest request)
    {
        var result = await productoService.CreateAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductoRequest request)
    {
        var result = await productoService.UpdateAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> Deactivate(int id)
    {
        var result = await productoService.DeactivateAsync(id);
        return ToHttpResponse(result);
    }

    [HttpPost("{id:int}/movimientos")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> RegistrarMovimiento(int id, [FromBody] CreateMovimientoStockRequest request)
    {
        var result = await productoService.RegistrarMovimientoAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpGet("{id:int}/movimientos")]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetMovimientos(int id)
    {
        var result = await productoService.GetMovimientosAsync(id);
        return ToHttpResponse(result);
    }

    [HttpGet("movimientos")]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetAllMovimientos(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? tipo = null,
        [FromQuery] string? estado = null)
    {
        var result = await productoService.GetAllMovimientosAsync(page, pageSize, tipo, estado);
        return ToHttpResponse(result);
    }

    [HttpPatch("movimientos/{id:int}/estado")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> AprobarRechazar(int id, [FromBody] AprobarRechazarMovimientoRequest request)
    {
        var result = await productoService.AprobarRechazarMovimientoAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpGet("movimientos/{id:int}/pdf")]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetMovimientoPdf(int id)
    {
        var result = await productoService.GetMovimientoByIdAsync(id);
        if (!result.IsSuccess)
            return ToHttpResponse(result);

        var pdf = pdfGenerator.Generate(result.Value!);
        return File(pdf, "application/pdf", $"movimiento-{id}.pdf");
    }

    [HttpPut("{id:int}/stock-config")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> UpdateStockConfig(int id, [FromBody] UpdateStockConfigRequest request)
    {
        var result = await productoService.UpdateStockConfigAsync(id, request);
        return ToHttpResponse(result);
    }

    // ── Imágenes ──────────────────────────────────────────────────────────────

    [HttpPost("{id:int}/imagen")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> UploadImagen(int id, IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { message = "No se recibió ningún archivo." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "El archivo no puede superar los 5 MB." });

        await using var stream = file.OpenReadStream();
        var result = await productoService.UploadImagenAsync(id, stream, file.FileName);
        return ToHttpResponse(result);
    }

    [HttpDelete("{id:int}/imagen")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> DeleteImagen(int id)
    {
        var result = await productoService.DeleteImagenAsync(id);
        return ToHttpResponse(result);
    }

    // ── Categorías ────────────────────────────────────────────────────────────

    [HttpGet("categorias")]
    [Authorize(Policy = "ver_inventario")]
    public async Task<IActionResult> GetCategorias()
    {
        var result = await productoService.GetCategoriasAsync();
        return ToHttpResponse(result);
    }

    [HttpPost("categorias")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> CreateCategoria([FromBody] CreateCategoriaProductoRequest request)
    {
        var result = await productoService.CreateCategoriaAsync(request);
        return ToHttpResponse(result);
    }

    [HttpPut("categorias/{id:int}")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> UpdateCategoria(int id, [FromBody] UpdateCategoriaProductoRequest request)
    {
        var result = await productoService.UpdateCategoriaAsync(id, request);
        return ToHttpResponse(result);
    }

    [HttpDelete("categorias/{id:int}")]
    [Authorize(Policy = "gestionar_inventario")]
    public async Task<IActionResult> DeactivateCategoria(int id)
    {
        var result = await productoService.DeactivateCategoriaAsync(id);
        return ToHttpResponse(result);
    }
}
