using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class ProductoService(AppDbContext db) : IProductoService
{
    public async Task<Result<PagedResult<ProductoResponse>>> GetAllAsync(
        int page, int pageSize, string? search, string? categoria, bool? bajoStock)
    {
        var query = db.Productos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(p =>
                p.Nombre.ToLower().Contains(q) ||
                (p.Sku != null && p.Sku.ToLower().Contains(q)));
        }

        if (!string.IsNullOrWhiteSpace(categoria))
            query = query.Where(p => p.Categoria == categoria);

        if (bajoStock == true)
            query = query.Where(p => p.StockActual <= p.StockMinimo && p.IsActive);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var discountMap = await db.CategoriasProducto
            .ToDictionaryAsync(c => c.Nombre, c => c.Descuento);

        var items = await query
            .Include(p => p.Marca)
            .Include(p => p.Modelo)
            .OrderBy(p => p.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<ProductoResponse>>.Success(new PagedResult<ProductoResponse>
        {
            Items      = items.Select(p => ToResponse(p, discountMap.GetValueOrDefault(p.Categoria, 0))),
            TotalCount = totalCount,
            TotalActive = await db.Productos.CountAsync(p => p.IsActive),
            Page       = page,
            PageSize   = pageSize,
            TotalPages = totalPages,
        });
    }

    public async Task<Result<ProductoResponse>> GetByIdAsync(int id)
    {
        var producto = await db.Productos
            .Include(p => p.Marca)
            .Include(p => p.Modelo)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (producto is null)
            return Result<ProductoResponse>.Failure("Producto no encontrado.", ErrorType.NotFound);

        var descuento = await db.CategoriasProducto
            .Where(c => c.Nombre == producto.Categoria)
            .Select(c => c.Descuento)
            .FirstOrDefaultAsync();

        return Result<ProductoResponse>.Success(ToResponse(producto, descuento));
    }

    public async Task<Result<ProductoResponse>> CreateAsync(CreateProductoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<ProductoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.Categoria))
            return Result<ProductoResponse>.Failure("La categoría es obligatoria.", ErrorType.Validation);

        if (request.PrecioCosto < 0 || request.PrecioVenta < 0)
            return Result<ProductoResponse>.Failure("Los precios no pueden ser negativos.", ErrorType.Validation);

        if (request.StockActual < 0 || request.StockMinimo < 0)
            return Result<ProductoResponse>.Failure("El stock no puede ser negativo.", ErrorType.Validation);

        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            var exists = await db.Productos.AnyAsync(p => p.Sku == request.Sku.Trim());
            if (exists)
                return Result<ProductoResponse>.Failure("Ya existe un producto con ese SKU.", ErrorType.Conflict);
        }

        var producto = new Producto
        {
            Nombre      = request.Nombre.Trim(),
            Categoria   = request.Categoria.Trim(),
            Sku         = request.Sku?.Trim(),
            PrecioCosto = request.PrecioCosto,
            PrecioVenta = request.PrecioVenta,
            StockActual = request.StockActual,
            StockMinimo = request.StockMinimo,
            MarcaId     = request.MarcaId,
            ModeloId    = request.ModeloId,
            Color       = request.Color?.Trim(),
            Talle       = request.Talle?.Trim(),
            Descripcion = request.Descripcion?.Trim(),
        };

        db.Productos.Add(producto);
        await db.SaveChangesAsync();

        await db.Entry(producto).Reference(p => p.Marca).LoadAsync();
        await db.Entry(producto).Reference(p => p.Modelo).LoadAsync();

        var descuentoCreate = await db.CategoriasProducto
            .Where(c => c.Nombre == producto.Categoria)
            .Select(c => c.Descuento)
            .FirstOrDefaultAsync();

        return Result<ProductoResponse>.Success(ToResponse(producto, descuentoCreate));
    }

    public async Task<Result<ProductoResponse>> UpdateAsync(int id, UpdateProductoRequest request)
    {
        var producto = await db.Productos.FindAsync(id);
        if (producto is null)
            return Result<ProductoResponse>.Failure("Producto no encontrado.", ErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<ProductoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.Categoria))
            return Result<ProductoResponse>.Failure("La categoría es obligatoria.", ErrorType.Validation);

        if (request.PrecioCosto < 0 || request.PrecioVenta < 0)
            return Result<ProductoResponse>.Failure("Los precios no pueden ser negativos.", ErrorType.Validation);

        if (!string.IsNullOrWhiteSpace(request.Sku) && request.Sku.Trim() != producto.Sku)
        {
            var exists = await db.Productos.AnyAsync(p => p.Sku == request.Sku.Trim() && p.Id != id);
            if (exists)
                return Result<ProductoResponse>.Failure("Ya existe un producto con ese SKU.", ErrorType.Conflict);
        }

        producto.Nombre      = request.Nombre.Trim();
        producto.Categoria   = request.Categoria.Trim();
        producto.Sku         = request.Sku?.Trim();
        producto.PrecioCosto = request.PrecioCosto;
        producto.PrecioVenta = request.PrecioVenta;
        producto.StockMinimo = request.StockMinimo;
        producto.IsActive    = request.IsActive;
        producto.MarcaId     = request.MarcaId;
        producto.ModeloId    = request.ModeloId;
        producto.Color       = request.Color?.Trim();
        producto.Talle       = request.Talle?.Trim();
        producto.Descripcion = request.Descripcion?.Trim();
        producto.UpdatedAt   = DateTime.UtcNow;

        await db.SaveChangesAsync();

        await db.Entry(producto).Reference(p => p.Marca).LoadAsync();
        await db.Entry(producto).Reference(p => p.Modelo).LoadAsync();

        var descuentoUpdate = await db.CategoriasProducto
            .Where(c => c.Nombre == producto.Categoria)
            .Select(c => c.Descuento)
            .FirstOrDefaultAsync();

        return Result<ProductoResponse>.Success(ToResponse(producto, descuentoUpdate));
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

    public async Task<Result<MovimientoStockResponse>> RegistrarMovimientoAsync(
        int productoId, CreateMovimientoStockRequest request)
    {
        var producto = await db.Productos.FindAsync(productoId);
        if (producto is null)
            return Result<MovimientoStockResponse>.Failure("Producto no encontrado.", ErrorType.NotFound);

        if (!new[] { "Entrada", "Salida", "Ajuste" }.Contains(request.Tipo))
            return Result<MovimientoStockResponse>.Failure("Tipo inválido. Use Entrada, Salida o Ajuste.", ErrorType.Validation);

        if (request.Cantidad <= 0)
            return Result<MovimientoStockResponse>.Failure("La cantidad debe ser mayor a cero.", ErrorType.Validation);

        if (request.Tipo == "Salida" && producto.StockActual < request.Cantidad)
            return Result<MovimientoStockResponse>.Failure("Stock insuficiente para registrar la salida.", ErrorType.Validation);

        var movimiento = new MovimientoStock
        {
            ProductoId = productoId,
            Tipo       = request.Tipo,
            Cantidad   = request.Cantidad,
            Motivo     = request.Motivo?.Trim(),
        };

        producto.StockActual = request.Tipo switch
        {
            "Entrada" => producto.StockActual + request.Cantidad,
            "Salida"  => producto.StockActual - request.Cantidad,
            "Ajuste"  => request.Cantidad,
            _         => producto.StockActual,
        };
        producto.UpdatedAt = DateTime.UtcNow;

        db.MovimientosStock.Add(movimiento);
        await db.SaveChangesAsync();

        return Result<MovimientoStockResponse>.Success(ToMovimientoResponse(movimiento, producto.Nombre));
    }

    public async Task<Result<IEnumerable<MovimientoStockResponse>>> GetMovimientosAsync(int productoId)
    {
        var producto = await db.Productos.FindAsync(productoId);
        if (producto is null)
            return Result<IEnumerable<MovimientoStockResponse>>.Failure("Producto no encontrado.", ErrorType.NotFound);

        var movimientos = await db.MovimientosStock
            .Where(m => m.ProductoId == productoId)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return Result<IEnumerable<MovimientoStockResponse>>.Success(
            movimientos.Select(m => ToMovimientoResponse(m, producto.Nombre)));
    }

    public async Task<Result<PagedResult<MovimientoStockResponse>>> GetAllMovimientosAsync(
        int page, int pageSize, string? tipo)
    {
        var query = db.MovimientosStock.Include(m => m.Producto).AsQueryable();

        if (!string.IsNullOrWhiteSpace(tipo))
            query = query.Where(m => m.Tipo == tipo);

        var totalCount = await query.CountAsync();

        var movimientos = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<MovimientoStockResponse>>.Success(new PagedResult<MovimientoStockResponse>
        {
            Items      = movimientos.Select(m => ToMovimientoResponse(m, m.Producto.Nombre)).ToList(),
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        });
    }

    private static ProductoResponse ToResponse(Producto p, decimal descuentoCategoria = 0) => new()
    {
        Id           = p.Id,
        Nombre       = p.Nombre,
        Categoria    = p.Categoria,
        Sku          = p.Sku,
        PrecioCosto  = p.PrecioCosto,
        PrecioVenta  = p.PrecioVenta,
        StockActual  = p.StockActual,
        StockMinimo  = p.StockMinimo,
        BajoStock    = p.StockActual <= p.StockMinimo,
        IsActive           = p.IsActive,
        DescuentoCategoria = descuentoCategoria,
        CreatedAt    = p.CreatedAt,
        UpdatedAt    = p.UpdatedAt,
        MarcaId      = p.MarcaId,
        MarcaNombre  = p.Marca?.Nombre,
        ModeloId     = p.ModeloId,
        ModeloNombre = p.Modelo?.Nombre,
        Color        = p.Color,
        Talle        = p.Talle,
        Descripcion  = p.Descripcion,
    };

    private static MovimientoStockResponse ToMovimientoResponse(MovimientoStock m, string productoNombre) => new()
    {
        Id             = m.Id,
        ProductoId     = m.ProductoId,
        ProductoNombre = productoNombre,
        Tipo           = m.Tipo,
        Cantidad       = m.Cantidad,
        Motivo         = m.Motivo,
        CreatedAt      = m.CreatedAt,
    };

    // ── Categorías de producto ─────────────────────────────────────────────────

    public async Task<Result<IEnumerable<CategoriaProductoResponse>>> GetCategoriasAsync()
    {
        var cats = await db.CategoriasProducto
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        var counts = await db.Productos
            .GroupBy(p => p.Categoria)
            .Select(g => new { Nombre = g.Key, Total = g.Count() })
            .ToListAsync();

        var result = cats.Select(c => new CategoriaProductoResponse
        {
            Id             = c.Id,
            Nombre         = c.Nombre,
            Descripcion    = c.Descripcion,
            Descuento      = c.Descuento,
            IsActive       = c.IsActive,
            TotalProductos = counts.FirstOrDefault(x => x.Nombre == c.Nombre)?.Total ?? 0,
        });

        return Result<IEnumerable<CategoriaProductoResponse>>.Success(result);
    }

    public async Task<Result<CategoriaProductoResponse>> CreateCategoriaAsync(CreateCategoriaProductoRequest request)
    {
        var nombre = request.Nombre.Trim();
        if (await db.CategoriasProducto.AnyAsync(c => c.Nombre == nombre))
            return Result<CategoriaProductoResponse>.Failure("Ya existe una categoría con ese nombre.", ErrorType.Conflict);

        if (request.Descuento < 0 || request.Descuento > 100)
            return Result<CategoriaProductoResponse>.Failure("El descuento debe estar entre 0 y 100.", ErrorType.Validation);

        var cat = new CategoriaProducto
        {
            Nombre      = nombre,
            Descripcion = request.Descripcion?.Trim(),
            Descuento   = request.Descuento,
            IsActive    = true,
            CreatedAt   = DateTime.UtcNow,
        };

        db.CategoriasProducto.Add(cat);
        await db.SaveChangesAsync();

        return Result<CategoriaProductoResponse>.Success(new CategoriaProductoResponse
        {
            Id = cat.Id, Nombre = cat.Nombre, Descripcion = cat.Descripcion,
            Descuento = cat.Descuento, IsActive = cat.IsActive, TotalProductos = 0,
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

        if (request.Descuento < 0 || request.Descuento > 100)
            return Result<CategoriaProductoResponse>.Failure("El descuento debe estar entre 0 y 100.", ErrorType.Validation);

        cat.Nombre      = nombre;
        cat.Descripcion = request.Descripcion?.Trim();
        cat.Descuento   = request.Descuento;
        cat.IsActive    = request.IsActive;

        await db.SaveChangesAsync();

        var total = await db.Productos.CountAsync(p => p.Categoria == cat.Nombre);
        return Result<CategoriaProductoResponse>.Success(new CategoriaProductoResponse
        {
            Id = cat.Id, Nombre = cat.Nombre, Descripcion = cat.Descripcion,
            Descuento = cat.Descuento, IsActive = cat.IsActive, TotalProductos = total,
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
}
