using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class CajaService(AppDbContext db) : ICajaService
{
    private static MovimientoCajaDto Map(MovimientoCaja m) => new()
    {
        Id         = m.Id,
        Tipo       = m.Tipo.ToString(),
        Monto      = m.Monto,
        Concepto   = m.Concepto,
        MetodoPago = m.MetodoPago.ToString(),
        VentaId    = m.VentaId,
        EgresoId   = m.EgresoId,
        Fecha      = m.Fecha.ToString("yyyy-MM-dd"),
        Referencia = m.Referencia,
        CreatedAt  = m.CreatedAt,
    };

    public async Task<Result<ResumenCajaDto>> GetResumenAsync(string fecha)
    {
        if (!DateOnly.TryParse(fecha, out var fechaParsed))
            return Result<ResumenCajaDto>.Failure("Fecha inválida", ErrorType.Validation);

        var movimientos = await db.MovimientosCaja
            .Where(m => m.Fecha == fechaParsed)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();

        var ingresos = movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Ingreso).ToList();
        var egresos  = movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Egreso).ToList();

        var cantidadVentas = await db.Ventas
            .CountAsync(v => v.FechaVenta == fechaParsed && v.Estado != EstadoVenta.Cancelada);

        var resumen = new ResumenCajaDto
        {
            Fecha           = fechaParsed.ToString("yyyy-MM-dd"),
            TotalIngresos   = ingresos.Sum(m => m.Monto),
            TotalEgresos    = egresos.Sum(m => m.Monto),
            SaldoNeto       = ingresos.Sum(m => m.Monto) - egresos.Sum(m => m.Monto),
            EfectivoTotal   = movimientos.Where(m => m.MetodoPago == MetodoPago.Efectivo && m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto),
            TarjetaTotal    = movimientos.Where(m => m.MetodoPago == MetodoPago.Tarjeta && m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto),
            TransferenciaTotal = movimientos.Where(m => m.MetodoPago == MetodoPago.Transferencia && m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto),
            ChequeTotal     = movimientos.Where(m => m.MetodoPago == MetodoPago.Cheque && m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto),
            CantidadVentas  = cantidadVentas,
            Movimientos     = movimientos.Select(Map).ToList(),
        };

        return Result<ResumenCajaDto>.Success(resumen);
    }

    public async Task<Result<PagedResult<MovimientoCajaDto>>> GetMovimientosAsync(
        string? fechaDesde, string? fechaHasta, string? tipo, int page, int pageSize)
    {
        var query = db.MovimientosCaja.AsQueryable();

        if (!string.IsNullOrWhiteSpace(fechaDesde) && DateOnly.TryParse(fechaDesde, out var desde))
            query = query.Where(m => m.Fecha >= desde);

        if (!string.IsNullOrWhiteSpace(fechaHasta) && DateOnly.TryParse(fechaHasta, out var hasta))
            query = query.Where(m => m.Fecha <= hasta);

        if (!string.IsNullOrWhiteSpace(tipo) && Enum.TryParse<TipoMovimientoCaja>(tipo, out var t))
            query = query.Where(m => m.Tipo == t);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<MovimientoCajaDto>>.Success(new PagedResult<MovimientoCajaDto>
        {
            Items      = items.Select(Map).ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        });
    }

    public async Task<Result<bool>> DeleteMovimientoAsync(int id)
    {
        var movimiento = await db.MovimientosCaja.FindAsync(id);
        if (movimiento == null) return Result<bool>.Failure("Movimiento no encontrado.", ErrorType.NotFound);

        db.MovimientosCaja.Remove(movimiento);
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }
}
