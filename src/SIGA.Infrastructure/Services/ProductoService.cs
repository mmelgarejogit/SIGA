using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Productos;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class ProductoService(AppDbContext db) : IProductoService
{
    // ── Productos ──────────────────────────────────────────────────────────────

    public async Task<Result<PagedResult<ProductoResponse>>> GetAllAsync(
        int page, int pageSize, string? search, int? categoriaId, bool? isActive)
    {
        var query = db.Productos
            .Include(p => p.CategoriaProducto)
            .Include(p => p.Marca)
            .Include(p => p.Modelo)
            .Include(p => p.Variantes)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(q));
        }

        if (categoriaId.HasValue)
            query = query.Where(p => p.CategoriaProductoId == categoriaId.Value);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<ProductoResponse>>.Success(new PagedResult<ProductoResponse>
        {
            Items      = items.Select(ToResponse),
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        });
    }

    public async Task<Result<ProductoResponse>> GetByIdAsync(int id)
    {
        var p = await db.Productos
            .Include(x => x.CategoriaProducto)
            .Include(x => x.Marca)
            .Include(x => x.Modelo)
            .Include(x => x.Variantes)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (p is null)
            return Result<ProductoResponse>.Failure("Producto no encontrado.", ErrorType.NotFound);
        return Result<ProductoResponse>.Success(ToResponse(p));
    }

    public async Task<Result<ProductoResponse>> CreateAsync(CreateProductoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<ProductoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        var producto = new Producto
        {
            Nombre            = request.Nombre.Trim(),
            Descripcion       = request.Descripcion?.Trim(),
            CategoriaProductoId = request.CategoriaProductoId,
            MarcaId           = request.MarcaId,
            ModeloId          = request.ModeloId,
        };

        db.Productos.Add(producto);
        await db.SaveChangesAsync();

        await db.Entry(producto).Reference(p => p.CategoriaProducto).LoadAsync();
        await db.Entry(producto).Reference(p => p.Marca).LoadAsync();
        await db.Entry(producto).Reference(p => p.Modelo).LoadAsync();

        return Result<ProductoResponse>.Success(ToResponse(producto));
    }

    public async Task<Result<ProductoResponse>> UpdateAsync(int id, UpdateProductoRequest request)
    {
        var producto = await db.Productos
            .Include(p => p.CategoriaProducto)
            .Include(p => p.Marca)
            .Include(p => p.Modelo)
            .Include(p => p.Variantes)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (producto is null)
            return Result<ProductoResponse>.Failure("Producto no encontrado.", ErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<ProductoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        producto.Nombre            = request.Nombre.Trim();
        producto.Descripcion       = request.Descripcion?.Trim();
        producto.CategoriaProductoId = request.CategoriaProductoId;
        producto.MarcaId           = request.MarcaId;
        producto.ModeloId          = request.ModeloId;
        producto.IsActive          = request.IsActive;
        producto.UpdatedAt         = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Result<ProductoResponse>.Success(ToResponse(producto));
    }

    public async Task<Result<bool>> DeactivateAsync(int id)
    {
        var producto = await db.Productos.FindAsync(id);
        if (producto is null)
            return Result<bool>.Failure("Producto no encontrado.", ErrorType.NotFound);
        producto.IsActive  = false;
        producto.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Variantes ──────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<ProductoVarianteResponse>>> GetVariantesAsync(int productoId)
    {
        var variantes = await db.ProductoVariantes
            .Include(v => v.Producto)
            .Where(v => v.ProductoId == productoId)
            .OrderBy(v => v.CreatedAt)
            .ToListAsync();
        return Result<IEnumerable<ProductoVarianteResponse>>.Success(variantes.Select(ToVarianteResponse));
    }

    public async Task<Result<ProductoVarianteResponse>> GetVarianteByIdAsync(Guid id)
    {
        var v = await db.ProductoVariantes.Include(x => x.Producto).FirstOrDefaultAsync(x => x.Id == id);
        if (v is null)
            return Result<ProductoVarianteResponse>.Failure("Variante no encontrada.", ErrorType.NotFound);
        return Result<ProductoVarianteResponse>.Success(ToVarianteResponse(v));
    }

    public async Task<Result<ProductoVarianteResponse>> CreateVarianteAsync(CreateProductoVarianteRequest request)
    {
        var producto = await db.Productos.FindAsync(request.ProductoId);
        if (producto is null)
            return Result<ProductoVarianteResponse>.Failure("Producto no encontrado.", ErrorType.NotFound);

        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            var exists = await db.ProductoVariantes.AnyAsync(v => v.Sku == request.Sku.Trim());
            if (exists)
                return Result<ProductoVarianteResponse>.Failure("Ya existe una variante con ese SKU.", ErrorType.Conflict);
        }

        var variante = new ProductoVariante
        {
            ProductoId  = request.ProductoId,
            Sku         = request.Sku?.Trim(),
            Color       = request.Color?.Trim(),
            Talle       = request.Talle?.Trim(),
            PrecioCosto = request.PrecioCosto,
            PrecioVenta = request.PrecioVenta,
        };

        db.ProductoVariantes.Add(variante);
        await db.SaveChangesAsync();

        variante.Producto = producto;
        return Result<ProductoVarianteResponse>.Success(ToVarianteResponse(variante));
    }

    public async Task<Result<ProductoVarianteResponse>> UpdateVarianteAsync(Guid id, UpdateProductoVarianteRequest request)
    {
        var variante = await db.ProductoVariantes.Include(v => v.Producto).FirstOrDefaultAsync(v => v.Id == id);
        if (variante is null)
            return Result<ProductoVarianteResponse>.Failure("Variante no encontrada.", ErrorType.NotFound);

        if (!string.IsNullOrWhiteSpace(request.Sku) && request.Sku.Trim() != variante.Sku)
        {
            var exists = await db.ProductoVariantes.AnyAsync(v => v.Sku == request.Sku.Trim() && v.Id != id);
            if (exists)
                return Result<ProductoVarianteResponse>.Failure("Ya existe una variante con ese SKU.", ErrorType.Conflict);
        }

        variante.Sku         = request.Sku?.Trim();
        variante.Color       = request.Color?.Trim();
        variante.Talle       = request.Talle?.Trim();
        variante.PrecioCosto = request.PrecioCosto;
        variante.PrecioVenta = request.PrecioVenta;
        variante.IsActive    = request.IsActive;
        variante.UpdatedAt   = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Result<ProductoVarianteResponse>.Success(ToVarianteResponse(variante));
    }

    public async Task<Result<bool>> DeactivateVarianteAsync(Guid id)
    {
        var variante = await db.ProductoVariantes.FindAsync(id);
        if (variante is null)
            return Result<bool>.Failure("Variante no encontrada.", ErrorType.NotFound);
        variante.IsActive  = false;
        variante.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    public async Task<Result<string>> UploadVarianteImagenAsync(Guid id, Stream stream, string fileName)
    {
        var variante = await db.ProductoVariantes.FindAsync(id);
        if (variante is null)
            return Result<string>.Failure("Variante no encontrada.", ErrorType.NotFound);

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            return Result<string>.Failure("Formato no soportado. Use JPG, PNG o WEBP.", ErrorType.Validation);

        var uploadsDir = Path.Combine("wwwroot", "uploads", "variantes");
        Directory.CreateDirectory(uploadsDir);

        if (!string.IsNullOrEmpty(variante.ImagenUrl))
        {
            var old = Path.Combine("wwwroot", variante.ImagenUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(old)) File.Delete(old);
        }

        var newFile = $"{id}{ext}";
        var path    = Path.Combine(uploadsDir, newFile);
        await using var fs = File.Create(path);
        await stream.CopyToAsync(fs);

        variante.ImagenUrl = $"/uploads/variantes/{newFile}";
        await db.SaveChangesAsync();
        return Result<string>.Success(variante.ImagenUrl);
    }

    // ── Categorías ─────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<CategoriaProductoResponse>>> GetCategoriasAsync()
    {
        var cats = await db.CategoriasProducto.OrderBy(c => c.Nombre).ToListAsync();
        var counts = await db.Productos
            .Where(p => p.CategoriaProductoId != null)
            .GroupBy(p => p.CategoriaProductoId)
            .Select(g => new { Id = g.Key, Total = g.Count() })
            .ToListAsync();

        var result = cats.Select(c => new CategoriaProductoResponse
        {
            Id             = c.Id,
            Nombre         = c.Nombre,
            Descripcion    = c.Descripcion,
            Margen         = c.Margen,
            Descuento      = c.Descuento,
            IsActive       = c.IsActive,
            TotalProductos = counts.FirstOrDefault(x => x.Id == c.Id)?.Total ?? 0,
        });

        return Result<IEnumerable<CategoriaProductoResponse>>.Success(result);
    }

    public async Task<Result<CategoriaProductoResponse>> CreateCategoriaAsync(CreateCategoriaProductoRequest request)
    {
        var nombre = request.Nombre.Trim();
        if (await db.CategoriasProducto.AnyAsync(c => c.Nombre == nombre))
            return Result<CategoriaProductoResponse>.Failure("Ya existe una categoría con ese nombre.", ErrorType.Conflict);

        var cat = new CategoriaProducto
        {
            Nombre      = nombre,
            Descripcion = request.Descripcion?.Trim(),
            Margen      = request.Margen,
            Descuento   = request.Descuento,
        };

        db.CategoriasProducto.Add(cat);
        await db.SaveChangesAsync();

        return Result<CategoriaProductoResponse>.Success(new CategoriaProductoResponse
        {
            Id = cat.Id, Nombre = cat.Nombre, Descripcion = cat.Descripcion,
            Margen = cat.Margen, Descuento = cat.Descuento, IsActive = cat.IsActive, TotalProductos = 0,
        });
    }

    public async Task<Result<CategoriaProductoResponse>> UpdateCategoriaAsync(int id, UpdateCategoriaProductoRequest request)
    {
        var cat = await db.CategoriasProducto.FindAsync(id);
        if (cat is null)
            return Result<CategoriaProductoResponse>.Failure("Categoría no encontrada.", ErrorType.NotFound);

        var nombre = request.Nombre.Trim();
        if (await db.CategoriasProducto.AnyAsync(c => c.Nombre == nombre && c.Id != id))
            return Result<CategoriaProductoResponse>.Failure("Ya existe una categoría con ese nombre.", ErrorType.Conflict);

        cat.Nombre      = nombre;
        cat.Descripcion = request.Descripcion?.Trim();
        cat.Margen      = request.Margen;
        cat.Descuento   = request.Descuento;
        cat.IsActive    = request.IsActive;

        await db.SaveChangesAsync();

        var total = await db.Productos.CountAsync(p => p.CategoriaProductoId == id);
        return Result<CategoriaProductoResponse>.Success(new CategoriaProductoResponse
        {
            Id = cat.Id, Nombre = cat.Nombre, Descripcion = cat.Descripcion,
            Margen = cat.Margen, Descuento = cat.Descuento, IsActive = cat.IsActive, TotalProductos = total,
        });
    }

    public async Task<Result<bool>> DeactivateCategoriaAsync(int id)
    {
        var cat = await db.CategoriasProducto.FindAsync(id);
        if (cat is null)
            return Result<bool>.Failure("Categoría no encontrada.", ErrorType.NotFound);
        cat.IsActive = false;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Mappers ────────────────────────────────────────────────────────────────

    private static ProductoResponse ToResponse(Producto p) => new()
    {
        Id                = p.Id,
        Nombre            = p.Nombre,
        Descripcion       = p.Descripcion,
        IsActive          = p.IsActive,
        CreatedAt         = p.CreatedAt,
        UpdatedAt         = p.UpdatedAt,
        CategoriaProductoId = p.CategoriaProductoId,
        CategoriaNombre   = p.CategoriaProducto?.Nombre,
        DescuentoCategoria = p.CategoriaProducto?.Descuento ?? 0,
        MarcaId           = p.MarcaId,
        MarcaNombre       = p.Marca?.Nombre,
        ModeloId          = p.ModeloId,
        ModeloNombre      = p.Modelo?.Nombre,
        TotalVariantes    = p.Variantes?.Count ?? 0,
    };

    private static ProductoVarianteResponse ToVarianteResponse(ProductoVariante v) => new()
    {
        Id             = v.Id,
        ProductoId     = v.ProductoId,
        ProductoNombre = v.Producto?.Nombre ?? "",
        Sku            = v.Sku,
        Color          = v.Color,
        Talle          = v.Talle,
        PrecioCosto    = v.PrecioCosto,
        PrecioVenta    = v.PrecioVenta,
        ImagenUrl      = v.ImagenUrl,
        IsActive       = v.IsActive,
        CreatedAt      = v.CreatedAt,
        UpdatedAt      = v.UpdatedAt,
    };
}
