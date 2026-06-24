using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class CajaService(AppDbContext db) : ICajaService
{
    // ── Mappers ──────────────────────────────────────────────────────────────────

    private static MovimientoCajaDto MapMov(MovimientoCaja m) => new()
    {
        Id                  = m.Id,
        Tipo                = m.Tipo.ToString(),
        Monto               = m.Monto,
        Concepto            = m.Concepto,
        MetodoPago          = m.MetodoPago.ToString(),
        VentaId             = m.VentaId,
        EgresoId            = m.EgresoId,
        SesionCajaId        = m.SesionCajaId,
        RegistradoPorNombre = m.RegistradoPor != null
            ? $"{m.RegistradoPor.Person?.FirstName} {m.RegistradoPor.Person?.LastName}".Trim()
            : null,
        Fecha      = m.Fecha.ToString("yyyy-MM-dd"),
        Referencia = m.Referencia,
        CreatedAt  = m.CreatedAt,
    };

    private static string NombreUsuario(User? u) =>
        u?.Person != null ? $"{u.Person.FirstName} {u.Person.LastName}".Trim() : "";

    private SesionCajaDto MapSesion(SesionCaja s, List<MovimientoCaja>? movimientos = null)
    {
        var movs = movimientos ?? s.Movimientos;
        var ingresos = movs.Where(m => m.Tipo == TipoMovimientoCaja.Ingreso).ToList();
        var egresos  = movs.Where(m => m.Tipo == TipoMovimientoCaja.Egreso).ToList();

        return new SesionCajaDto
        {
            Id                 = s.Id,
            Estado             = s.Estado.ToString(),
            MontoInicial       = s.MontoInicial,
            AbiertaPorNombre   = NombreUsuario(s.AbiertaPor),
            FechaApertura      = s.FechaApertura,
            CerradaPorNombre   = s.CerradaPor != null ? NombreUsuario(s.CerradaPor) : null,
            FechaCierre        = s.FechaCierre,
            EfectivoContado    = s.EfectivoContado,
            EfectivoEsperado   = s.EfectivoEsperado,
            Diferencia         = s.Diferencia,
            ObservacionCierre  = s.ObservacionCierre,
            AprobadoPorNombre  = s.AprobadoPor != null ? NombreUsuario(s.AprobadoPor) : null,
            FechaAprobacion    = s.FechaAprobacion,
            MotivoRechazo      = s.MotivoRechazo,
            TotalIngresos      = ingresos.Sum(m => m.Monto),
            TotalEgresos       = egresos.Sum(m => m.Monto),
            SaldoNeto          = ingresos.Sum(m => m.Monto) - egresos.Sum(m => m.Monto),
            EfectivoIngresos   = ingresos.Where(m => m.MetodoPago == MetodoPago.Efectivo).Sum(m => m.Monto),
            TarjetaTotal       = ingresos.Where(m => m.MetodoPago == MetodoPago.Tarjeta).Sum(m => m.Monto),
            TransferenciaTotal = ingresos.Where(m => m.MetodoPago == MetodoPago.Transferencia).Sum(m => m.Monto),
            ChequeTotal        = ingresos.Where(m => m.MetodoPago == MetodoPago.Cheque).Sum(m => m.Monto),
            CantidadMovimientos = movs.Count,
            Movimientos        = movs.Select(MapMov).ToList(),
        };
    }

    private static SesionCajaListDto MapSesionList(SesionCaja s) => new()
    {
        Id                = s.Id,
        Estado            = s.Estado.ToString(),
        MontoInicial      = s.MontoInicial,
        AbiertaPorNombre  = NombreUsuario(s.AbiertaPor),
        FechaApertura     = s.FechaApertura,
        CerradaPorNombre  = s.CerradaPor != null ? NombreUsuario(s.CerradaPor) : null,
        FechaCierre       = s.FechaCierre,
        EfectivoContado   = s.EfectivoContado,
        EfectivoEsperado  = s.EfectivoEsperado,
        Diferencia        = s.Diferencia,
        AprobadoPorNombre = s.AprobadoPor != null ? NombreUsuario(s.AprobadoPor) : null,
        FechaAprobacion   = s.FechaAprobacion,
        MotivoRechazo     = s.MotivoRechazo,
        TotalIngresos     = s.Movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto),
        TotalEgresos      = s.Movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Egreso).Sum(m => m.Monto),
        SaldoNeto         = s.Movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto)
                          - s.Movimientos.Where(m => m.Tipo == TipoMovimientoCaja.Egreso).Sum(m => m.Monto),
    };

    // ── Sesión de caja ────────────────────────────────────────────────────────────

    private const decimal Tolerancia = 50_000m;

    private IQueryable<SesionCaja> SesionQuery() =>
        db.SesionesCaja
            .Include(s => s.AbiertaPor).ThenInclude(u => u!.Person)
            .Include(s => s.CerradaPor).ThenInclude(u => u!.Person)
            .Include(s => s.AprobadoPor).ThenInclude(u => u!.Person)
            .Include(s => s.Movimientos).ThenInclude(m => m.RegistradoPor).ThenInclude(u => u!.Person);

    public async Task<Result<SesionCajaDto?>> GetSesionActualAsync()
    {
        var sesion = await SesionQuery()
            .FirstOrDefaultAsync(s =>
                s.Estado == EstadoSesionCaja.Abierta ||
                s.Estado == EstadoSesionCaja.PendienteAprobacion);

        return Result<SesionCajaDto?>.Success(sesion == null ? null : MapSesion(sesion));
    }

    /// <summary>
    /// Efectivo con el que debería abrir la próxima caja: el conteo físico (EfectivoContado)
    /// del último cierre. El efectivo del cajón se traslada de una sesión a la siguiente.
    /// </summary>
    private async Task<decimal> MontoAperturaSugeridoAsync() =>
        await db.SesionesCaja
            .Where(s => s.Estado == EstadoSesionCaja.Cerrada && s.EfectivoContado != null)
            .OrderByDescending(s => s.FechaCierre)
            .Select(s => s.EfectivoContado!.Value)
            .FirstOrDefaultAsync();

    public async Task<Result<decimal>> GetMontoAperturaSugeridoAsync() =>
        Result<decimal>.Success(await MontoAperturaSugeridoAsync());

    public async Task<Result<SesionCajaDto>> AbrirSesionAsync(AbrirSesionRequest request, int userId)
    {
        if (request.MontoInicial is < 0)
            return Result<SesionCajaDto>.Failure("El monto inicial no puede ser negativo", ErrorType.Validation);

        var yaAbierta = await db.SesionesCaja.AnyAsync(s =>
            s.Estado == EstadoSesionCaja.Abierta ||
            s.Estado == EstadoSesionCaja.PendienteAprobacion);
        if (yaAbierta)
            return Result<SesionCajaDto>.Failure("Ya hay una caja abierta", ErrorType.Conflict);

        // Apertura automática: sin monto explícito se arranca con el efectivo del último cierre.
        var montoInicial = request.MontoInicial ?? await MontoAperturaSugeridoAsync();

        var sesion = new SesionCaja
        {
            Estado        = EstadoSesionCaja.Abierta,
            MontoInicial  = montoInicial,
            AbiertaPorId  = userId,
            FechaApertura = DateTime.UtcNow,
        };

        db.SesionesCaja.Add(sesion);
        await db.SaveChangesAsync();

        var created = await SesionQuery().FirstAsync(s => s.Id == sesion.Id);
        return Result<SesionCajaDto>.Success(MapSesion(created));
    }

    public async Task<Result<SesionCajaDto>> GetSesionByIdAsync(int id)
    {
        var sesion = await SesionQuery().FirstOrDefaultAsync(s => s.Id == id);
        if (sesion == null)
            return Result<SesionCajaDto>.Failure("Sesión no encontrada", ErrorType.NotFound);

        return Result<SesionCajaDto>.Success(MapSesion(sesion));
    }

    public async Task<Result<SesionCajaDto>> CerrarSesionAsync(int id, CerrarSesionRequest request, int userId)
    {
        var sesion = await SesionQuery().FirstOrDefaultAsync(s => s.Id == id);
        if (sesion == null)
            return Result<SesionCajaDto>.Failure("Sesión no encontrada", ErrorType.NotFound);
        if (sesion.Estado != EstadoSesionCaja.Abierta)
            return Result<SesionCajaDto>.Failure("La sesión no está abierta", ErrorType.Conflict);

        var movimientos = sesion.Movimientos;
        var ingresosEfectivo = movimientos
            .Where(m => m.Tipo == TipoMovimientoCaja.Ingreso && m.MetodoPago == MetodoPago.Efectivo)
            .Sum(m => m.Monto);
        var egresosEfectivo = movimientos
            .Where(m => m.Tipo == TipoMovimientoCaja.Egreso && m.MetodoPago == MetodoPago.Efectivo)
            .Sum(m => m.Monto);

        var efectivoEsperado = sesion.MontoInicial + ingresosEfectivo - egresosEfectivo;
        var diferencia        = request.EfectivoContado - Math.Max(0m, efectivoEsperado);

        sesion.CerradaPorId      = userId;
        sesion.FechaCierre       = DateTime.UtcNow;
        sesion.EfectivoContado   = request.EfectivoContado;
        sesion.EfectivoEsperado  = efectivoEsperado;
        sesion.Diferencia        = diferencia;
        sesion.ObservacionCierre = request.Observacion;
        sesion.Estado            = Math.Abs(diferencia) <= Tolerancia
            ? EstadoSesionCaja.Cerrada
            : EstadoSesionCaja.PendienteAprobacion;

        await db.SaveChangesAsync();

        var updated = await SesionQuery().FirstAsync(s => s.Id == id);
        return Result<SesionCajaDto>.Success(MapSesion(updated));
    }

    public async Task<Result<SesionCajaDto>> AprobarCierreAsync(int id, int userId)
    {
        var sesion = await SesionQuery().FirstOrDefaultAsync(s => s.Id == id);
        if (sesion == null)
            return Result<SesionCajaDto>.Failure("Sesión no encontrada", ErrorType.NotFound);
        if (sesion.Estado != EstadoSesionCaja.PendienteAprobacion)
            return Result<SesionCajaDto>.Failure("La sesión no está pendiente de aprobación", ErrorType.Conflict);

        sesion.Estado          = EstadoSesionCaja.Cerrada;
        sesion.AprobadoPorId   = userId;
        sesion.FechaAprobacion = DateTime.UtcNow;

        await db.SaveChangesAsync();
        var updated = await SesionQuery().FirstAsync(s => s.Id == id);
        return Result<SesionCajaDto>.Success(MapSesion(updated));
    }

    public async Task<Result<SesionCajaDto>> RechazarCierreAsync(int id, RechazarCierreRequest request, int userId)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
            return Result<SesionCajaDto>.Failure("El motivo de rechazo es obligatorio", ErrorType.Validation);

        var sesion = await SesionQuery().FirstOrDefaultAsync(s => s.Id == id);
        if (sesion == null)
            return Result<SesionCajaDto>.Failure("Sesión no encontrada", ErrorType.NotFound);
        if (sesion.Estado != EstadoSesionCaja.PendienteAprobacion)
            return Result<SesionCajaDto>.Failure("La sesión no está pendiente de aprobación", ErrorType.Conflict);

        // Vuelve a Abierta para que el cajero pueda re-arquear
        sesion.Estado           = EstadoSesionCaja.Abierta;
        sesion.MotivoRechazo    = request.Motivo.Trim();
        sesion.CerradaPorId     = null;
        sesion.FechaCierre      = null;
        sesion.EfectivoContado  = null;
        sesion.EfectivoEsperado = null;
        sesion.Diferencia       = null;
        sesion.ObservacionCierre = null;

        await db.SaveChangesAsync();
        var updated = await SesionQuery().FirstAsync(s => s.Id == id);
        return Result<SesionCajaDto>.Success(MapSesion(updated));
    }

    public async Task<Result<PagedResult<SesionCajaListDto>>> GetSesionesAsync(int page, int pageSize, string? estado = null)
    {
        pageSize = Math.Min(pageSize, 100);

        var baseQuery = db.SesionesCaja.AsQueryable();
        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoSesionCaja>(estado, out var estadoEnum))
            baseQuery = baseQuery.Where(s => s.Estado == estadoEnum);

        var query = baseQuery
            .Include(s => s.AbiertaPor).ThenInclude(u => u!.Person)
            .Include(s => s.CerradaPor).ThenInclude(u => u!.Person)
            .Include(s => s.AprobadoPor).ThenInclude(u => u!.Person)
            .Include(s => s.Movimientos)
            .OrderByDescending(s => s.FechaApertura)
            .AsSplitQuery();

        var total = await baseQuery.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Result<PagedResult<SesionCajaListDto>>.Success(new PagedResult<SesionCajaListDto>
        {
            Items      = items.Select(MapSesionList).ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        });
    }

    // ── Resumen / movimientos (existentes, adaptados) ─────────────────────────────

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
            Fecha              = fechaParsed.ToString("yyyy-MM-dd"),
            TotalIngresos      = ingresos.Sum(m => m.Monto),
            TotalEgresos       = egresos.Sum(m => m.Monto),
            SaldoNeto          = ingresos.Sum(m => m.Monto) - egresos.Sum(m => m.Monto),
            EfectivoTotal      = movimientos.Where(m => m.MetodoPago == MetodoPago.Efectivo && m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto),
            TarjetaTotal       = movimientos.Where(m => m.MetodoPago == MetodoPago.Tarjeta && m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto),
            TransferenciaTotal = movimientos.Where(m => m.MetodoPago == MetodoPago.Transferencia && m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto),
            ChequeTotal        = movimientos.Where(m => m.MetodoPago == MetodoPago.Cheque && m.Tipo == TipoMovimientoCaja.Ingreso).Sum(m => m.Monto),
            CantidadVentas     = cantidadVentas,
            Movimientos        = movimientos.Select(MapMov).ToList(),
        };

        return Result<ResumenCajaDto>.Success(resumen);
    }

    public async Task<Result<PagedResult<MovimientoCajaDto>>> GetMovimientosAsync(
        string? fechaDesde, string? fechaHasta, string? tipo, int page, int pageSize)
    {
        var query = db.MovimientosCaja
            .Include(m => m.RegistradoPor).ThenInclude(u => u!.Person)
            .AsQueryable();

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
            Items      = items.Select(MapMov).ToList(),
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
