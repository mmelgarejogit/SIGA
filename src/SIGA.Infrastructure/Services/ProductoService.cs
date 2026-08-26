using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;
using System.Security.Claims;

namespace SIGA.Infrastructure.Services;

public class ProductoService(AppDbContext db, IHttpContextAccessor http, ICurrentUserContext current) : IProductoService
{
    private string? CurrentUserId =>
        http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    private string? CurrentUserName =>
        http.HttpContext?.User.FindFirstValue("name");

    private async Task<int> StockDeProductoAsync(int productoId)
    {
        return await db.StockActual
            .Where(s => s.ProductoId == productoId)
            .SumAsync(s => (int?)s.StockActual) ?? 0;
    }

    public async Task<Result<PagedResult<ProductoResponse>>> GetAllAsync(
        int page, int pageSize, string? search, string? categoria, bool? bajoStock, string? tipoCategoria)
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

        if (!string.IsNullOrWhiteSpace(tipoCategoria) && Enum.TryParse<TipoCategoriaProducto>(tipoCategoria, true, out var tc))
        {
            var nombresTipo = await db.CategoriasProducto.Where(c => c.Tipo == tc).Select(c => c.Nombre).ToListAsync();
            query = query.Where(p => nombresTipo.Contains(p.Categoria));
        }

        if (bajoStock == true)
        {
            query = query.Where(p =>
                (db.StockActual
                    .Where(s => s.ProductoId == p.Id)
                    .Sum(s => (int?)s.StockActual) ?? 0)
                <=
                (db.ProductosStockConfig
                    .Where(c => c.ProductoId == p.Id)
                    .Select(c => (int?)c.StockMinimo)
                    .FirstOrDefault() ?? 0)
                && p.IsActive);
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var cats = await db.CategoriasProducto.ToListAsync();
        var discountMap = cats.ToDictionary(c => c.Nombre, c => c.Descuento);
        var tipoMap     = cats.ToDictionary(c => c.Nombre, c => c.Tipo);

        var productos = await query
            .Include(p => p.Marca)
            .Include(p => p.Modelo)
            .Include(p => p.StockConfig)
            .OrderBy(p => p.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var productoIds = productos.Select(p => p.Id).ToList();
        var stockMap = await db.StockActual
            .Where(s => productoIds.Contains(s.ProductoId))
            .GroupBy(s => s.ProductoId)
            .Select(g => new { ProductoId = g.Key, Stock = g.Sum(x => x.StockActual) })
            .ToDictionaryAsync(x => x.ProductoId, x => x.Stock);

        return Result<PagedResult<ProductoResponse>>.Success(new PagedResult<ProductoResponse>
        {
            Items = productos.Select(p => ToResponse(
                p,
                discountMap.GetValueOrDefault(p.Categoria, 0),
                stockMap.GetValueOrDefault(p.Id, 0),
                tipoMap.GetValueOrDefault(p.Categoria, TipoCategoriaProducto.Generico))),
            TotalCount  = totalCount,
            TotalActive = await db.Productos.CountAsync(p => p.IsActive),
            Page        = page,
            PageSize    = pageSize,
            TotalPages  = totalPages,
        });
    }

    public async Task<Result<ProductoResponse>> GetByIdAsync(int id)
    {
        var producto = await db.Productos
            .Include(p => p.Marca)
            .Include(p => p.Modelo)
            .Include(p => p.StockConfig)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (producto is null)
            return Result<ProductoResponse>.Failure("Producto no encontrado.", ErrorType.NotFound);

        var catById = await db.CategoriasProducto
            .Where(c => c.Nombre == producto.Categoria)
            .Select(c => new { c.Descuento, c.Tipo })
            .FirstOrDefaultAsync();

        var stockActual = await StockDeProductoAsync(id);

        return Result<ProductoResponse>.Success(ToResponse(
            producto, catById?.Descuento ?? 0, stockActual, catById?.Tipo ?? TipoCategoriaProducto.Generico));
    }

    public async Task<Result<ProductoResponse>> CreateAsync(CreateProductoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<ProductoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.Categoria))
            return Result<ProductoResponse>.Failure("La categoría es obligatoria.", ErrorType.Validation);

        if (request.PrecioCosto < 0)
            return Result<ProductoResponse>.Failure("El precio de costo no puede ser negativo.", ErrorType.Validation);

        if (request.StockMinimo < 0)
            return Result<ProductoResponse>.Failure("El stock mínimo no puede ser negativo.", ErrorType.Validation);

        if (!string.IsNullOrWhiteSpace(request.Sku))
        {
            var exists = await db.Productos.AnyAsync(p => p.Sku == request.Sku.Trim());
            if (exists)
                return Result<ProductoResponse>.Failure("Ya existe un producto con ese SKU.", ErrorType.Conflict);
        }

        var catCreate = await db.CategoriasProducto
            .Where(c => c.Nombre == request.Categoria.Trim())
            .Select(c => new { c.Margen, c.Descuento, c.Tipo })
            .FirstOrDefaultAsync();

        var producto = new Producto
        {
            Nombre      = request.Nombre.Trim(),
            Categoria   = request.Categoria.Trim(),
            Sku         = request.Sku?.Trim(),
            MarcaId     = request.MarcaId,
            ModeloId    = request.ModeloId,
            Color       = request.Color?.Trim(),
            Talle       = request.Talle?.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            StockConfig = new ProductoStockConfig
            {
                StockMinimo = request.StockMinimo,
                UpdatedAt   = DateTime.UtcNow,
            },
        };
        // El precio de venta se deriva del costo + margen de la categoría (no se carga a mano).
        producto.AplicarCosto(request.PrecioCosto, catCreate?.Margen ?? 0);

        db.Productos.Add(producto);
        await db.SaveChangesAsync();

        await db.Entry(producto).Reference(p => p.Marca).LoadAsync();
        await db.Entry(producto).Reference(p => p.Modelo).LoadAsync();

        return Result<ProductoResponse>.Success(ToResponse(
            producto, catCreate?.Descuento ?? 0, 0, catCreate?.Tipo ?? TipoCategoriaProducto.Generico));
    }

    public async Task<Result<ProductoResponse>> UpdateAsync(int id, UpdateProductoRequest request)
    {
        var producto = await db.Productos
            .Include(p => p.StockConfig)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (producto is null)
            return Result<ProductoResponse>.Failure("Producto no encontrado.", ErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<ProductoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.Categoria))
            return Result<ProductoResponse>.Failure("La categoría es obligatoria.", ErrorType.Validation);

        if (request.PrecioCosto < 0)
            return Result<ProductoResponse>.Failure("El precio de costo no puede ser negativo.", ErrorType.Validation);

        if (!string.IsNullOrWhiteSpace(request.Sku) && request.Sku.Trim() != producto.Sku)
        {
            var exists = await db.Productos.AnyAsync(p => p.Sku == request.Sku.Trim() && p.Id != id);
            if (exists)
                return Result<ProductoResponse>.Failure("Ya existe un producto con ese SKU.", ErrorType.Conflict);
        }

        var catUpdate = await db.CategoriasProducto
            .Where(c => c.Nombre == request.Categoria.Trim())
            .Select(c => new { c.Margen, c.Descuento, c.Tipo })
            .FirstOrDefaultAsync();

        producto.Nombre      = request.Nombre.Trim();
        producto.Categoria   = request.Categoria.Trim();
        producto.Sku         = request.Sku?.Trim();
        // El precio de venta se deriva del costo + margen de la categoría (no se carga a mano).
        producto.AplicarCosto(request.PrecioCosto, catUpdate?.Margen ?? 0);
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

        var stockActualUpdate = await StockDeProductoAsync(id);

        return Result<ProductoResponse>.Success(ToResponse(
            producto, catUpdate?.Descuento ?? 0, stockActualUpdate, catUpdate?.Tipo ?? TipoCategoriaProducto.Generico));
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

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var producto = await db.Productos
            .Include(p => p.StockConfig)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (producto is null)
            return Result<bool>.Failure("Producto no encontrado.", ErrorType.NotFound);

        var enUso =
            await db.MovimientosStock.AnyAsync(m => m.ProductoId == id) ||
            await db.VentaLineas.AnyAsync(l => l.ProductoId == id) ||
            await db.TrabajosPedido.AnyAsync(t => t.ArmazonProductoId == id);
        if (enUso)
            return Result<bool>.Failure(
                "No se puede eliminar: el producto tiene movimientos de stock o ventas asociadas. Desactivalo en su lugar.",
                ErrorType.Conflict);

        if (producto.StockConfig is not null)
            db.ProductosStockConfig.Remove(producto.StockConfig);
        db.Productos.Remove(producto);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Red de seguridad ante otras FKs (compras, conteos, devoluciones, lotes…).
            return Result<bool>.Failure(
                "No se puede eliminar: el producto está referenciado en otros registros. Desactivalo en su lugar.",
                ErrorType.Conflict);
        }
        return Result<bool>.Success(true);
    }

    public async Task<Result<MovimientoStockResponse>> RegistrarMovimientoAsync(
        int productoId, CreateMovimientoStockRequest request)
    {
        var producto = await db.Productos.FindAsync(productoId);
        if (producto is null)
            return Result<MovimientoStockResponse>.Failure("Producto no encontrado.", ErrorType.NotFound);

        if (!Enum.TryParse<TipoMovimientoStock>(request.Tipo, ignoreCase: true, out var tipoCreate))
            return Result<MovimientoStockResponse>.Failure("Tipo inválido. Use Entrada, Salida o Ajuste.", ErrorType.Validation);

        if (request.Cantidad <= 0)
            return Result<MovimientoStockResponse>.Failure("La cantidad debe ser mayor a cero.", ErrorType.Validation);

        string? motivoNombre = null;
        if (request.MotivoMovimientoId.HasValue)
        {
            var motivo = await db.MotivosMovimiento.FindAsync(request.MotivoMovimientoId.Value);
            if (motivo is null)
                return Result<MovimientoStockResponse>.Failure("Motivo no encontrado.", ErrorType.NotFound);
            motivoNombre = motivo.Nombre;
        }

        var movimiento = new MovimientoStock
        {
            ProductoId         = productoId,
            SucursalId         = await SucursalResolver.WriteBranchAsync(db, current),
            Tipo               = tipoCreate,
            Cantidad           = request.Cantidad,
            Motivo             = motivoNombre,
            MotivoMovimientoId = request.MotivoMovimientoId,
            FechaMovimiento    = request.FechaMovimiento?.ToUniversalTime() ?? DateTime.UtcNow,
            CreadoPorId        = CurrentUserId,
            CreadoPorNombre    = CurrentUserName,
            Estado             = EstadoMovimientoStock.Pendiente,
        };

        db.MovimientosStock.Add(movimiento);
        await db.SaveChangesAsync();

        return Result<MovimientoStockResponse>.Success(ToMovimientoResponse(movimiento, producto.Nombre));
    }

    public async Task<Result<MovimientoStockResponse>> AprobarRechazarMovimientoAsync(
        int id, AprobarRechazarMovimientoRequest request)
    {
        if (!Enum.TryParse<EstadoMovimientoStock>(request.Estado, ignoreCase: true, out var estadoParsed)
            || (estadoParsed != EstadoMovimientoStock.Aprobado && estadoParsed != EstadoMovimientoStock.Rechazado))
            return Result<MovimientoStockResponse>.Failure("Estado inválido.", ErrorType.Validation);

        var movimiento = await db.MovimientosStock
            .Include(m => m.Producto)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (movimiento is null)
            return Result<MovimientoStockResponse>.Failure("Movimiento no encontrado.", ErrorType.NotFound);

        if (movimiento.Estado != EstadoMovimientoStock.Pendiente)
            return Result<MovimientoStockResponse>.Failure("Solo se pueden gestionar movimientos en estado Pendiente.", ErrorType.Validation);

        if (estadoParsed == EstadoMovimientoStock.Aprobado)
        {
            var stockActual = await db.StockActual
                .Where(s => s.ProductoId == movimiento.ProductoId && s.SucursalId == movimiento.SucursalId)
                .SumAsync(s => (int?)s.StockActual) ?? 0;

            if (movimiento.Tipo == TipoMovimientoStock.Salida && stockActual < movimiento.Cantidad)
                return Result<MovimientoStockResponse>.Failure("Stock insuficiente para aprobar la salida.", ErrorType.Validation);

            if (movimiento.Tipo == TipoMovimientoStock.Ajuste)
            {
                // Convertir Ajuste a delta: registrar movimiento compensatorio aprobado
                var delta = movimiento.Cantidad - stockActual;
                if (delta != 0)
                {
                    var compensatorio = new MovimientoStock
                    {
                        ProductoId      = movimiento.ProductoId,
                        SucursalId      = movimiento.SucursalId,
                        Tipo            = delta > 0 ? TipoMovimientoStock.Entrada : TipoMovimientoStock.Salida,
                        Cantidad        = Math.Abs(delta),
                        Motivo          = $"Ajuste #{movimiento.Id}",
                        FechaMovimiento = DateTime.UtcNow,
                        CreadoPorId     = CurrentUserId,
                        CreadoPorNombre = CurrentUserName,
                        Estado                  = EstadoMovimientoStock.Aprobado,
                        AprobadoPorNombre       = CurrentUserName,
                        FechaAprobacion         = DateTime.UtcNow,
                        ObservacionesAprobacion = request.Observaciones?.Trim(),
                    };
                    db.MovimientosStock.Add(compensatorio);
                }
            }
        }

        movimiento.Estado                  = estadoParsed;
        movimiento.AprobadoPorNombre       = CurrentUserName;
        movimiento.FechaAprobacion         = DateTime.UtcNow;
        movimiento.ObservacionesAprobacion = request.Observaciones?.Trim();

        await db.SaveChangesAsync();

        return Result<MovimientoStockResponse>.Success(ToMovimientoResponse(movimiento, movimiento.Producto.Nombre));
    }

    public async Task<Result<MovimientoStockResponse>> GetMovimientoByIdAsync(int id)
    {
        var m = await db.MovimientosStock.Include(x => x.Producto).FirstOrDefaultAsync(x => x.Id == id);
        if (m is null)
            return Result<MovimientoStockResponse>.Failure("Movimiento no encontrado.", ErrorType.NotFound);
        return Result<MovimientoStockResponse>.Success(ToMovimientoResponse(m, m.Producto.Nombre));
    }

    public async Task<Result<IEnumerable<MovimientoStockResponse>>> GetMovimientosAsync(int productoId)
    {
        var producto = await db.Productos.FindAsync(productoId);
        if (producto is null)
            return Result<IEnumerable<MovimientoStockResponse>>.Failure("Producto no encontrado.", ErrorType.NotFound);

        var movimientosQuery = db.MovimientosStock
            .Include(m => m.Sucursal)
            .Where(m => m.ProductoId == productoId);

        var movimientos = await movimientosQuery
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync();

        return Result<IEnumerable<MovimientoStockResponse>>.Success(
            movimientos.Select(m => ToMovimientoResponse(m, producto.Nombre)));
    }

    public async Task<Result<PagedResult<MovimientoStockResponse>>> GetAllMovimientosAsync(
        int page, int pageSize, string? tipo, string? estado)
    {
        var query = db.MovimientosStock.Include(m => m.Producto).Include(m => m.Sucursal).AsQueryable();

        if (!string.IsNullOrWhiteSpace(tipo) && Enum.TryParse<TipoMovimientoStock>(tipo, ignoreCase: true, out var tipoFiltro))
            query = query.Where(m => m.Tipo == tipoFiltro);

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoMovimientoStock>(estado, ignoreCase: true, out var estadoFiltro))
            query = query.Where(m => m.Estado == estadoFiltro);

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

    public async Task<Result<ProductoResponse>> UpdateStockConfigAsync(int id, UpdateStockConfigRequest request)
    {
        var producto = await db.Productos
            .Include(p => p.Marca)
            .Include(p => p.Modelo)
            .Include(p => p.StockConfig)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (producto is null)
            return Result<ProductoResponse>.Failure("Producto no encontrado.", ErrorType.NotFound);

        if (request.PrecioCosto < 0)
            return Result<ProductoResponse>.Failure("El precio de costo no puede ser negativo.", ErrorType.Validation);

        if (request.StockMinimo < 0)
            return Result<ProductoResponse>.Failure("El stock mínimo no puede ser negativo.", ErrorType.Validation);

        if (request.StockMaximo.HasValue && request.StockMaximo < request.StockMinimo)
            return Result<ProductoResponse>.Failure("El stock máximo no puede ser menor al stock mínimo.", ErrorType.Validation);

        var margen = await db.CategoriasProducto
            .Where(c => c.Nombre == producto.Categoria)
            .Select(c => c.Margen)
            .FirstOrDefaultAsync();

        producto.AplicarCosto(request.PrecioCosto, margen);
        producto.UpdatedAt = DateTime.UtcNow;

        if (producto.StockConfig is null)
        {
            producto.StockConfig = new ProductoStockConfig
            {
                ProductoId  = id,
                StockMinimo = request.StockMinimo,
                StockMaximo = request.StockMaximo,
                UpdatedAt   = DateTime.UtcNow,
            };
        }
        else
        {
            producto.StockConfig.StockMinimo = request.StockMinimo;
            producto.StockConfig.StockMaximo = request.StockMaximo;
            producto.StockConfig.UpdatedAt   = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        var catStock = await db.CategoriasProducto
            .Where(c => c.Nombre == producto.Categoria)
            .Select(c => new { c.Descuento, c.Tipo })
            .FirstOrDefaultAsync();

        var stockActual = await StockDeProductoAsync(id);

        return Result<ProductoResponse>.Success(ToResponse(
            producto, catStock?.Descuento ?? 0, stockActual, catStock?.Tipo ?? TipoCategoriaProducto.Generico));
    }

    public async Task<Result<string>> UploadImagenAsync(int id, Stream stream, string fileName)
    {
        var producto = await db.Productos.FindAsync(id);
        if (producto is null)
            return Result<string>.Failure("Producto no encontrado.", ErrorType.NotFound);

        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext is not (".jpg" or ".jpeg" or ".png" or ".webp"))
            return Result<string>.Failure("Formato no soportado. Use JPG, PNG o WEBP.", ErrorType.Validation);

        var uploadsDir = Path.Combine("wwwroot", "uploads", "productos");
        Directory.CreateDirectory(uploadsDir);

        if (!string.IsNullOrEmpty(producto.ImagenUrl))
        {
            var old = Path.Combine("wwwroot", producto.ImagenUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(old)) File.Delete(old);
        }

        var newFile = $"{id}{ext}";
        var path = Path.Combine(uploadsDir, newFile);
        await using var fs = File.Create(path);
        await stream.CopyToAsync(fs);

        producto.ImagenUrl = $"/uploads/productos/{newFile}";
        await db.SaveChangesAsync();

        return Result<string>.Success(producto.ImagenUrl);
    }

    public async Task<Result<bool>> DeleteImagenAsync(int id)
    {
        var producto = await db.Productos.FindAsync(id);
        if (producto is null)
            return Result<bool>.Failure("Producto no encontrado.", ErrorType.NotFound);

        if (!string.IsNullOrEmpty(producto.ImagenUrl))
        {
            var path = Path.Combine("wwwroot", producto.ImagenUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(path)) File.Delete(path);
        }

        producto.ImagenUrl = null;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static ProductoResponse ToResponse(Producto p, decimal descuentoCategoria, int stockActual, TipoCategoriaProducto tipoCategoria) => new()
    {
        Id                 = p.Id,
        Nombre             = p.Nombre,
        Categoria          = p.Categoria,
        TipoCategoria      = tipoCategoria.ToString(),
        Sku                = p.Sku,
        PrecioCosto        = p.PrecioCosto,
        PrecioVenta        = p.PrecioVenta,
        StockActual        = stockActual,
        StockMinimo        = p.StockConfig?.StockMinimo ?? 0,
        StockMaximo        = p.StockConfig?.StockMaximo,
        BajoStock          = stockActual <= (p.StockConfig?.StockMinimo ?? 0),
        IsActive           = p.IsActive,
        DescuentoCategoria = descuentoCategoria,
        CreatedAt          = p.CreatedAt,
        UpdatedAt          = p.UpdatedAt,
        MarcaId            = p.MarcaId,
        MarcaNombre        = p.Marca?.Nombre,
        ModeloId           = p.ModeloId,
        ModeloNombre       = p.Modelo?.Nombre,
        Color              = p.Color,
        Talle              = p.Talle,
        Descripcion        = p.Descripcion,
        ImagenUrl          = p.ImagenUrl,
    };

    private static MovimientoStockResponse ToMovimientoResponse(MovimientoStock m, string productoNombre) => new()
    {
        Id                      = m.Id,
        ProductoId              = m.ProductoId,
        ProductoNombre          = productoNombre,
        SucursalId              = m.SucursalId,
        SucursalNombre          = m.Sucursal?.Nombre,
        Tipo                    = m.Tipo.ToString(),
        Cantidad                = m.Cantidad,
        Motivo                  = m.Motivo,
        MotivoMovimientoId      = m.MotivoMovimientoId,
        FechaMovimiento         = m.FechaMovimiento,
        CreadoPorNombre         = m.CreadoPorNombre,
        Estado                  = m.Estado.ToString(),
        AprobadoPorNombre       = m.AprobadoPorNombre,
        FechaAprobacion         = m.FechaAprobacion,
        ObservacionesAprobacion = m.ObservacionesAprobacion,
        CreatedAt               = m.CreatedAt,
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
            Tipo           = c.Tipo.ToString(),
            Margen         = c.Margen,
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

        if (request.Margen < 0 || request.Margen > 1000)
            return Result<CategoriaProductoResponse>.Failure("El margen debe estar entre 0 y 1000.", ErrorType.Validation);

        if (request.Descuento < 0 || request.Descuento > 100)
            return Result<CategoriaProductoResponse>.Failure("El descuento debe estar entre 0 y 100.", ErrorType.Validation);

        var tipo = Enum.TryParse<TipoCategoriaProducto>(request.Tipo, true, out var t) ? t : TipoCategoriaProducto.Generico;

        var cat = new CategoriaProducto
        {
            Nombre      = nombre,
            Descripcion = request.Descripcion?.Trim(),
            Tipo        = tipo,
            Margen      = request.Margen,
            Descuento   = request.Descuento,
            IsActive    = true,
            CreatedAt   = DateTime.UtcNow,
        };

        db.CategoriasProducto.Add(cat);
        await db.SaveChangesAsync();

        return Result<CategoriaProductoResponse>.Success(new CategoriaProductoResponse
        {
            Id = cat.Id, Nombre = cat.Nombre, Descripcion = cat.Descripcion, Tipo = cat.Tipo.ToString(),
            Margen = cat.Margen, Descuento = cat.Descuento, IsActive = cat.IsActive, TotalProductos = 0,
        });
    }

    public async Task<Result<CategoriaProductoResponse>> UpdateCategoriaAsync(int id, UpdateCategoriaProductoRequest request)
    {
        var cat = await db.CategoriasProducto.FindAsync(id);
        if (cat is null)
            return Result<CategoriaProductoResponse>.Failure("Categoría no encontrada.", ErrorType.NotFound);

        var oldNombre = cat.Nombre;
        var nombre = request.Nombre.Trim();
        if (await db.CategoriasProducto.AnyAsync(c => c.Nombre == nombre && c.Id != id))
            return Result<CategoriaProductoResponse>.Failure("Ya existe una categoría con ese nombre.", ErrorType.Conflict);

        if (request.Margen < 0 || request.Margen > 1000)
            return Result<CategoriaProductoResponse>.Failure("El margen debe estar entre 0 y 1000.", ErrorType.Validation);

        if (request.Descuento < 0 || request.Descuento > 100)
            return Result<CategoriaProductoResponse>.Failure("El descuento debe estar entre 0 y 100.", ErrorType.Validation);

        cat.Nombre      = nombre;
        cat.Descripcion = request.Descripcion?.Trim();
        cat.Tipo        = Enum.TryParse<TipoCategoriaProducto>(request.Tipo, true, out var t) ? t : TipoCategoriaProducto.Generico;
        cat.Margen      = request.Margen;
        cat.Descuento   = request.Descuento;
        cat.IsActive    = request.IsActive;

        await db.SaveChangesAsync();

        // Re-vincular el campo string legado al nuevo nombre y recalcular el precio de venta
        // de los productos de esta categoría con el nuevo margen. Sin el re-vínculo, los
        // productos quedan buscando una categoría con el nombre viejo (que ya no existe) en
        // cada lookup por nombre posterior, incluido el margen=0 por defecto de AplicarCosto.
        var productosCat = await db.Productos.Where(p => p.Categoria == oldNombre).ToListAsync();
        foreach (var prod in productosCat)
        {
            prod.Categoria = nombre;
            prod.AplicarCosto(prod.PrecioCosto, cat.Margen);
        }
        if (productosCat.Count > 0)
            await db.SaveChangesAsync();

        var total = await db.Productos.CountAsync(p => p.Categoria == cat.Nombre);
        return Result<CategoriaProductoResponse>.Success(new CategoriaProductoResponse
        {
            Id = cat.Id, Nombre = cat.Nombre, Descripcion = cat.Descripcion, Tipo = cat.Tipo.ToString(),
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

    public async Task<Result<bool>> DeleteCategoriaAsync(int id)
    {
        var cat = await db.CategoriasProducto.FindAsync(id);
        if (cat is null)
            return Result<bool>.Failure("Categoría no encontrada.", ErrorType.NotFound);

        var enUso = await db.Productos.AnyAsync(p => p.CategoriaProductoId == id || p.Categoria == cat.Nombre);
        if (enUso)
            return Result<bool>.Failure(
                "No se puede eliminar: hay productos en esta categoría. Desactivala en su lugar.",
                ErrorType.Conflict);

        db.CategoriasProducto.Remove(cat);
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }
}
