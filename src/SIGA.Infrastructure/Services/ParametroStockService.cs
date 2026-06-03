using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Stock;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class ParametroStockService(AppDbContext db) : IParametroStockService
{
    public async Task<Result<IEnumerable<ParametroStockResponse>>> GetAllAsync(
        Guid? sucursalId, Guid? productoVarianteId)
    {
        var query = db.ParametrosStock
            .Include(p => p.ProductoVariante).ThenInclude(v => v.Producto)
            .Include(p => p.Sucursal)
            .AsQueryable();

        if (sucursalId.HasValue)
            query = query.Where(p => p.SucursalId == sucursalId.Value);

        if (productoVarianteId.HasValue)
            query = query.Where(p => p.ProductoVarianteId == productoVarianteId.Value);

        var items = await query.ToListAsync();
        return Result<IEnumerable<ParametroStockResponse>>.Success(items.Select(ToResponse));
    }

    public async Task<Result<ParametroStockResponse>> UpsertAsync(UpsertParametroStockRequest request)
    {
        if (request.StockMinimo < 0 || request.StockMaximo < 0)
            return Result<ParametroStockResponse>.Failure("Los valores no pueden ser negativos.", ErrorType.Validation);

        if (request.StockMaximo > 0 && request.StockMaximo < request.StockMinimo)
            return Result<ParametroStockResponse>.Failure("El stock máximo debe ser mayor o igual al mínimo.", ErrorType.Validation);

        var variante = await db.ProductoVariantes.Include(v => v.Producto).FirstOrDefaultAsync(v => v.Id == request.ProductoVarianteId);
        if (variante is null)
            return Result<ParametroStockResponse>.Failure("Variante no encontrada.", ErrorType.NotFound);

        var sucursal = await db.Sucursales.FindAsync(request.SucursalId);
        if (sucursal is null)
            return Result<ParametroStockResponse>.Failure("Sucursal no encontrada.", ErrorType.NotFound);

        var existing = await db.ParametrosStock.FindAsync(request.ProductoVarianteId, request.SucursalId);

        if (existing is null)
        {
            existing = new ParametroStock
            {
                ProductoVarianteId = request.ProductoVarianteId,
                SucursalId         = request.SucursalId,
                StockMinimo        = request.StockMinimo,
                StockMaximo        = request.StockMaximo,
            };
            db.ParametrosStock.Add(existing);
        }
        else
        {
            existing.StockMinimo = request.StockMinimo;
            existing.StockMaximo = request.StockMaximo;
        }

        await db.SaveChangesAsync();

        existing.ProductoVariante = variante;
        existing.Sucursal         = sucursal;

        return Result<ParametroStockResponse>.Success(ToResponse(existing));
    }

    public async Task<Result<bool>> DeleteAsync(Guid productoVarianteId, Guid sucursalId)
    {
        var param = await db.ParametrosStock.FindAsync(productoVarianteId, sucursalId);
        if (param is null)
            return Result<bool>.Failure("Parámetro no encontrado.", ErrorType.NotFound);

        db.ParametrosStock.Remove(param);
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static ParametroStockResponse ToResponse(ParametroStock p) => new()
    {
        ProductoVarianteId = p.ProductoVarianteId,
        ProductoNombre     = p.ProductoVariante?.Producto?.Nombre ?? "",
        VarianteSku        = p.ProductoVariante?.Sku,
        VarianteColor      = p.ProductoVariante?.Color,
        VarianteTalle      = p.ProductoVariante?.Talle,
        SucursalId         = p.SucursalId,
        SucursalNombre     = p.Sucursal?.Nombre ?? "",
        StockMinimo        = p.StockMinimo,
        StockMaximo        = p.StockMaximo,
    };
}
