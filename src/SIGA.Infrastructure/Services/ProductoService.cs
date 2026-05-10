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

        var items = await query
            .OrderBy(p => p.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<ProductoResponse>>.Success(new PagedResult<ProductoResponse>
        {
            Items      = items.Select(ToResponse),
            TotalCount = totalCount,
            TotalActive = await db.Productos.CountAsync(p => p.IsActive),
            Page       = page,
            PageSize   = pageSize,
            TotalPages = totalPages,
        });
    }

    public async Task<Result<ProductoResponse>> GetByIdAsync(int id)
    {
        var producto = await db.Productos.FindAsync(id);
        if (producto is null)
            return Result<ProductoResponse>.Failure("Producto no encontrado.", ErrorType.NotFound);

        return Result<ProductoResponse>.Success(ToResponse(producto));
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
        };

        db.Productos.Add(producto);
        await db.SaveChangesAsync();

        return Result<ProductoResponse>.Success(ToResponse(producto));
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
        producto.UpdatedAt   = DateTime.UtcNow;

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

    public async Task<Result<IEnumerable<MovimientoStockResponse>>> GetAllMovimientosAsync(
        int page, int pageSize, string? tipo)
    {
        var query = db.MovimientosStock.Include(m => m.Producto).AsQueryable();

        if (!string.IsNullOrWhiteSpace(tipo))
            query = query.Where(m => m.Tipo == tipo);

        var movimientos = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<IEnumerable<MovimientoStockResponse>>.Success(
            movimientos.Select(m => ToMovimientoResponse(m, m.Producto.Nombre)));
    }

    private static ProductoResponse ToResponse(Producto p) => new()
    {
        Id          = p.Id,
        Nombre      = p.Nombre,
        Categoria   = p.Categoria,
        Sku         = p.Sku,
        PrecioCosto = p.PrecioCosto,
        PrecioVenta = p.PrecioVenta,
        StockActual = p.StockActual,
        StockMinimo = p.StockMinimo,
        BajoStock   = p.StockActual <= p.StockMinimo,
        IsActive    = p.IsActive,
        CreatedAt   = p.CreatedAt,
        UpdatedAt   = p.UpdatedAt,
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
}
