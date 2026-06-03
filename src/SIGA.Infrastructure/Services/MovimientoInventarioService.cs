using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Stock;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class MovimientoInventarioService(AppDbContext db) : IMovimientoInventarioService
{
    public async Task<Result<PagedResult<MovimientoInventarioResponse>>> GetAllAsync(
        int page, int pageSize, Guid? sucursalId, Guid? productoVarianteId,
        string? tipo, string? origen)
    {
        var query = db.MovimientosInventario
            .Include(m => m.ProductoVariante).ThenInclude(v => v.Producto)
            .Include(m => m.Sucursal)
            .Include(m => m.Usuario).ThenInclude(u => u.Person)
            .Include(m => m.TipoAjuste)
            .AsQueryable();

        if (sucursalId.HasValue)
            query = query.Where(m => m.SucursalId == sucursalId.Value);

        if (productoVarianteId.HasValue)
            query = query.Where(m => m.ProductoVarianteId == productoVarianteId.Value);

        if (!string.IsNullOrWhiteSpace(tipo) && Enum.TryParse<TipoMovimiento>(tipo, out var t))
            query = query.Where(m => m.Tipo == t);

        if (!string.IsNullOrWhiteSpace(origen) && Enum.TryParse<OrigenMovimiento>(origen, out var o))
            query = query.Where(m => m.OrigenTipo == o);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.Fecha)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<MovimientoInventarioResponse>>.Success(new PagedResult<MovimientoInventarioResponse>
        {
            Items      = items.Select(ToResponse),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        });
    }

    public async Task<Result<IEnumerable<StockPorVarianteResponse>>> GetStockAsync(
        Guid? sucursalId, Guid? productoVarianteId, bool? bajoStock)
    {
        var query = db.MovimientosInventario
            .Include(m => m.ProductoVariante).ThenInclude(v => v.Producto)
            .Include(m => m.Sucursal)
            .AsQueryable();

        if (sucursalId.HasValue)
            query = query.Where(m => m.SucursalId == sucursalId.Value);
        if (productoVarianteId.HasValue)
            query = query.Where(m => m.ProductoVarianteId == productoVarianteId.Value);

        var movimientos = await query.ToListAsync();

        var parametros = await db.ParametrosStock
            .Where(p => (!sucursalId.HasValue || p.SucursalId == sucursalId.Value)
                     && (!productoVarianteId.HasValue || p.ProductoVarianteId == productoVarianteId.Value))
            .ToListAsync();

        var stock = movimientos
            .GroupBy(m => new { m.ProductoVarianteId, m.SucursalId })
            .Select(g =>
            {
                var first   = g.First();
                var param   = parametros.FirstOrDefault(p => p.ProductoVarianteId == g.Key.ProductoVarianteId
                                                          && p.SucursalId == g.Key.SucursalId);
                var actual  = g.Sum(m => m.Tipo == TipoMovimiento.Ingreso ? m.Cantidad : -m.Cantidad);
                return new StockPorVarianteResponse
                {
                    ProductoVarianteId = g.Key.ProductoVarianteId,
                    ProductoNombre     = first.ProductoVariante?.Producto?.Nombre ?? "",
                    Sku                = first.ProductoVariante?.Sku,
                    Color              = first.ProductoVariante?.Color,
                    Talle              = first.ProductoVariante?.Talle,
                    SucursalId         = g.Key.SucursalId,
                    SucursalNombre     = first.Sucursal?.Nombre ?? "",
                    StockActual        = actual,
                    StockMinimo        = param?.StockMinimo,
                    StockMaximo        = param?.StockMaximo,
                };
            })
            .ToList();

        if (bajoStock == true)
            stock = stock.Where(s => s.BajoStock).ToList();

        return Result<IEnumerable<StockPorVarianteResponse>>.Success(stock);
    }

    public async Task<Result<int>> GetStockActualAsync(Guid productoVarianteId, Guid sucursalId)
    {
        var total = await db.MovimientosInventario
            .Where(m => m.ProductoVarianteId == productoVarianteId && m.SucursalId == sucursalId)
            .SumAsync(m => m.Tipo == TipoMovimiento.Ingreso ? m.Cantidad : -m.Cantidad);

        return Result<int>.Success(total);
    }

    internal static MovimientoInventarioResponse ToResponse(MovimientoInventario m) => new()
    {
        Id                 = m.Id,
        ProductoVarianteId = m.ProductoVarianteId,
        ProductoNombre     = m.ProductoVariante?.Producto?.Nombre ?? "",
        VarianteSku        = m.ProductoVariante?.Sku,
        VarianteColor      = m.ProductoVariante?.Color,
        VarianteTalle      = m.ProductoVariante?.Talle,
        SucursalId         = m.SucursalId,
        SucursalNombre     = m.Sucursal?.Nombre ?? "",
        Tipo               = m.Tipo.ToString(),
        Cantidad           = m.Cantidad,
        Fecha              = m.Fecha,
        UsuarioId          = m.UsuarioId,
        UsuarioNombre      = $"{m.Usuario?.Person?.FirstName} {m.Usuario?.Person?.LastName}".Trim(),
        OrigenTipo         = m.OrigenTipo.ToString(),
        ReferenciaId       = m.ReferenciaId,
        TipoAjusteNombre   = m.TipoAjuste?.Nombre,
    };
}
