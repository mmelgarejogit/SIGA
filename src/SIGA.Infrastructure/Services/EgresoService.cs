using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Egresos;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class EgresoService(AppDbContext db) : IEgresoService
{
    private static EgresoResponse Map(Egreso e, EgresoPago? pago = null)
    {
        var r = new EgresoResponse
        {
            Id               = e.Id,
            SucursalId       = e.SucursalId,
            CreadoPorUserId   = e.CreadoPorUserId,
            FechaCreacion     = e.FechaCreacion,
            Tipo             = e.Tipo.ToString(),
            Estado           = e.Estado.ToString(),
            Monto            = e.Monto,
            Concepto         = e.Concepto,
            Observaciones    = e.Observaciones,
            FechaEmision     = e.FechaEmision.ToString("yyyy-MM-dd"),
            FechaVencimiento = e.FechaVencimiento?.ToString("yyyy-MM-dd"),
            FechaPago        = e.FechaPago?.ToString("yyyy-MM-dd"),
            MetodoPago       = e.MetodoPago?.ToString(),
            EstaVencido      = e.EstaVencido(),
            MotivoRechazo    = e.MotivoRechazo,
            AprobadoPorUserId = e.AprobadoPorUserId,
            FechaAprobacion  = e.FechaAprobacion?.ToString("yyyy-MM-dd"),
            NroComprobante   = e.NroComprobante,
            CreatedAt        = e.CreatedAt,
        };

        if (e.Sucursal != null)
            r.SucursalNombre = e.Sucursal.Nombre;
        if (e.CreadoPorUser?.Person != null)
            r.CreadoPorUserNombre = $"{e.CreadoPorUser.Person.FirstName} {e.CreadoPorUser.Person.LastName}".Trim();
        if (e.AprobadoPorUser?.Person != null)
            r.AprobadoPorUserNombre = $"{e.AprobadoPorUser.Person.FirstName} {e.AprobadoPorUser.Person.LastName}".Trim();

        if (pago != null)
        {
            r.EgresoPagoId = pago.Id;
            r.EgresoPagoFechaPago = pago.FechaPago.ToString("yyyy-MM-dd");
            r.EgresoPagoMetodoPago = pago.MetodoPago.ToString();
            r.EgresoPagoNumeroComprobante = pago.NumeroComprobante;
            r.EgresoPagoObservaciones = pago.Observaciones;
            r.EgresoPagoRegistradoPorUserId = pago.RegistradoPorUserId;
            if (pago.RegistradoPorUser?.Person != null)
                r.EgresoPagoRegistradoPorUserNombre = $"{pago.RegistradoPorUser.Person.FirstName} {pago.RegistradoPorUser.Person.LastName}".Trim();
        }

        if (e is FacturaCompra fc)
        {
            r.NroFactura        = fc.NroFactura;
            r.ProveedorId       = fc.ProveedorId;
            r.ProveedorNombre   = fc.Proveedor?.Nombre;
            r.PedidoProveedorId = fc.PedidoProveedorId;
            r.MontoExento       = fc.MontoExento;
            r.MontoGravado5     = fc.MontoGravado5;
            r.MontoGravado10    = fc.MontoGravado10;
            r.Iva5              = fc.Iva5;
            r.Iva10             = fc.Iva10;
            r.MontoTotal        = fc.MontoTotal;
            r.CondicionVenta    = fc.CondicionVenta.ToString();
        }
        else if (e is Honorario h)
        {
            r.ProfessionalId     = h.ProfessionalId;
            r.ProfessionalNombre = $"{h.Professional?.User?.Person?.FirstName} {h.Professional?.User?.Person?.LastName}".Trim();
            r.PeriodoMes          = h.PeriodoMes;
            r.PeriodoAnio         = h.PeriodoAnio;
        }
        else if (e is GastoGeneral g)
        {
            r.CategoriaGastoId     = g.CategoriaGastoId;
            r.CategoriaGastoNombre = g.CategoriaGasto?.Nombre;
        }
        else if (e is SalarioEmpleado s)
        {
            r.EmpleadoId     = s.EmpleadoId;
            r.EmpleadoNombre = $"{s.Empleado?.User?.Person?.FirstName} {s.Empleado?.User?.Person?.LastName}".Trim();
            r.PeriodoMes     = s.PeriodoMes;
            r.PeriodoAnio    = s.PeriodoAnio;
        }

        return r;
    }

    private static CategoriaGastoResponse MapCategoria(CategoriaGasto c) => new()
    {
        Id          = c.Id,
        Nombre      = c.Nombre,
        Descripcion = c.Descripcion,
        Activo      = c.Activo,
    };

    private IQueryable<Egreso> BaseQuery() =>
        db.Egresos
            .Include(e => e.Sucursal)
            .Include(e => e.CreadoPorUser).ThenInclude(u => u!.Person)
            .Include(e => e.AprobadoPorUser).ThenInclude(u => u!.Person)
            .Include(e => (e as FacturaCompra)!.Proveedor)
            .Include(e => (e as Honorario)!.Professional)
                .ThenInclude(p => p!.User)
                    .ThenInclude(u => u!.Person)
            .Include(e => (e as GastoGeneral)!.CategoriaGasto)
            .Include(e => (e as SalarioEmpleado)!.Empleado)
                .ThenInclude(emp => emp!.User)
                    .ThenInclude(u => u!.Person);

    public async Task<Result<IEnumerable<CategoriaGastoResponse>>> GetCategoriasAsync()
    {
        var cats = await db.CategoriasGasto.OrderBy(c => c.Nombre).ToListAsync();
        return Result<IEnumerable<CategoriaGastoResponse>>.Success(cats.Select(MapCategoria));
    }

    public async Task<Result<CategoriaGastoResponse>> CrearCategoriaAsync(CrearCategoriaGastoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<CategoriaGastoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        var categoria = new CategoriaGasto
        {
            Nombre      = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
        };
        db.CategoriasGasto.Add(categoria);
        await db.SaveChangesAsync();
        return Result<CategoriaGastoResponse>.Success(MapCategoria(categoria));
    }

    public async Task<Result<CategoriaGastoResponse>> ActualizarCategoriaAsync(int id, ActualizarCategoriaGastoRequest request)
    {
        var categoria = await db.CategoriasGasto.FindAsync(id);
        if (categoria is null)
            return Result<CategoriaGastoResponse>.Failure("Categoría no encontrada.", ErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<CategoriaGastoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        categoria.Nombre      = request.Nombre.Trim();
        categoria.Descripcion = request.Descripcion?.Trim();
        categoria.Activo      = request.Activo;
        await db.SaveChangesAsync();
        return Result<CategoriaGastoResponse>.Success(MapCategoria(categoria));
    }

    public async Task<Result<EgresoResponse>> CrearFacturaCompraAsync(CrearFacturaCompraRequest request, int userId)
    {
        return Result<EgresoResponse>.Failure(
            "No se pueden crear facturas de compra nuevas. Este tipo es solo de lectura.",
            ErrorType.Validation);
    }

    public async Task<Result<EgresoResponse>> CrearHonorarioAsync(CrearHonorarioRequest request, int userId)
    {
        if (request.SucursalId == Guid.Empty)
            return Result<EgresoResponse>.Failure("La sucursal es obligatoria.", ErrorType.Validation);

        if (!DateOnly.TryParse(request.FechaEmision, out var fechaEmision))
            return Result<EgresoResponse>.Failure("Fecha de emisión inválida.", ErrorType.Validation);

        if (request.PeriodoMes < 1 || request.PeriodoMes > 12)
            return Result<EgresoResponse>.Failure("El mes del período debe estar entre 1 y 12.", ErrorType.Validation);

        var sucursal = await db.Sucursales.FindAsync(request.SucursalId);
        if (sucursal is null)
            return Result<EgresoResponse>.Failure("Sucursal no encontrada.", ErrorType.NotFound);

        DateOnly? fechaVencimiento = null;
        if (!string.IsNullOrWhiteSpace(request.FechaVencimiento))
        {
            if (!DateOnly.TryParse(request.FechaVencimiento, out var fv))
                return Result<EgresoResponse>.Failure("Fecha de vencimiento inválida.", ErrorType.Validation);
            fechaVencimiento = fv;
        }

        var professional = await db.Professionals
            .Include(p => p.User).ThenInclude(u => u.Person)
            .FirstOrDefaultAsync(p => p.Id == request.ProfessionalId);
        if (professional is null)
            return Result<EgresoResponse>.Failure("Profesional no encontrado.", ErrorType.NotFound);

        var honorario = new Honorario
        {
            SucursalId     = request.SucursalId,
            CreadoPorUserId = userId,
            FechaCreacion   = DateTime.UtcNow,
            ProfessionalId = request.ProfessionalId,
            Monto          = request.Monto,
            Concepto       = request.Concepto.Trim(),
            PeriodoMes     = request.PeriodoMes,
            PeriodoAnio    = request.PeriodoAnio,
            Observaciones  = request.Observaciones?.Trim(),
            FechaEmision   = fechaEmision,
            FechaVencimiento = fechaVencimiento,
            Estado         = EstadoEgreso.Pendiente,
        };
        db.Honorarios.Add(honorario);
        await db.SaveChangesAsync();

        honorario.Professional = professional;
        honorario.Sucursal = sucursal;
        return Result<EgresoResponse>.Success(Map(honorario));
    }

    public async Task<Result<EgresoResponse>> CrearGastoGeneralAsync(CrearGastoGeneralRequest request, int userId)
    {
        if (request.SucursalId == Guid.Empty)
            return Result<EgresoResponse>.Failure("La sucursal es obligatoria.", ErrorType.Validation);

        if (!DateOnly.TryParse(request.FechaEmision, out var fechaEmision))
            return Result<EgresoResponse>.Failure("Fecha de emisión inválida.", ErrorType.Validation);

        var sucursal = await db.Sucursales.FindAsync(request.SucursalId);
        if (sucursal is null)
            return Result<EgresoResponse>.Failure("Sucursal no encontrada.", ErrorType.NotFound);

        DateOnly? fechaVencimiento = null;
        if (!string.IsNullOrWhiteSpace(request.FechaVencimiento))
        {
            if (!DateOnly.TryParse(request.FechaVencimiento, out var fv))
                return Result<EgresoResponse>.Failure("Fecha de vencimiento inválida.", ErrorType.Validation);
            fechaVencimiento = fv;
        }

        var categoria = await db.CategoriasGasto.FindAsync(request.CategoriaGastoId);
        if (categoria is null)
            return Result<EgresoResponse>.Failure("Categoría no encontrada.", ErrorType.NotFound);

        var gasto = new GastoGeneral
        {
            SucursalId       = request.SucursalId,
            CreadoPorUserId  = userId,
            FechaCreacion    = DateTime.UtcNow,
            CategoriaGastoId = request.CategoriaGastoId,
            Monto            = request.Monto,
            Concepto         = request.Concepto.Trim(),
            Observaciones    = request.Observaciones?.Trim(),
            FechaEmision     = fechaEmision,
            FechaVencimiento = fechaVencimiento,
            Estado           = EstadoEgreso.Pendiente,
        };
        db.GastosGenerales.Add(gasto);
        await db.SaveChangesAsync();

        gasto.CategoriaGasto = categoria;
        gasto.Sucursal = sucursal;
        return Result<EgresoResponse>.Success(Map(gasto));
    }

    public async Task<Result<EgresoResponse>> CrearSalarioAsync(CrearSalarioRequest request, int userId)
    {
        if (request.SucursalId == Guid.Empty)
            return Result<EgresoResponse>.Failure("La sucursal es obligatoria.", ErrorType.Validation);

        if (!DateOnly.TryParse(request.FechaEmision, out var fechaEmision))
            return Result<EgresoResponse>.Failure("Fecha de emisión inválida.", ErrorType.Validation);

        if (request.PeriodoMes < 1 || request.PeriodoMes > 12)
            return Result<EgresoResponse>.Failure("El mes del período debe estar entre 1 y 12.", ErrorType.Validation);

        var sucursal = await db.Sucursales.FindAsync(request.SucursalId);
        if (sucursal is null)
            return Result<EgresoResponse>.Failure("Sucursal no encontrada.", ErrorType.NotFound);

        DateOnly? fechaVencimiento = null;
        if (!string.IsNullOrWhiteSpace(request.FechaVencimiento))
        {
            if (!DateOnly.TryParse(request.FechaVencimiento, out var fv))
                return Result<EgresoResponse>.Failure("Fecha de vencimiento inválida.", ErrorType.Validation);
            fechaVencimiento = fv;
        }

        var empleado = await db.Empleados
            .Include(e => e.User).ThenInclude(u => u.Person)
            .FirstOrDefaultAsync(e => e.Id == request.EmpleadoId && e.IsActive);
        if (empleado is null)
            return Result<EgresoResponse>.Failure("Empleado no encontrado.", ErrorType.NotFound);

        var salario = new SalarioEmpleado
        {
            SucursalId     = request.SucursalId,
            CreadoPorUserId = userId,
            FechaCreacion  = DateTime.UtcNow,
            EmpleadoId     = request.EmpleadoId,
            Monto          = request.Monto,
            Concepto       = request.Concepto.Trim(),
            PeriodoMes     = request.PeriodoMes,
            PeriodoAnio    = request.PeriodoAnio,
            Observaciones  = request.Observaciones?.Trim(),
            FechaEmision   = fechaEmision,
            FechaVencimiento = fechaVencimiento,
            Estado         = EstadoEgreso.Pendiente,
        };
        db.SalariosEmpleado.Add(salario);
        await db.SaveChangesAsync();

        salario.Empleado = empleado;
        salario.Sucursal = sucursal;
        return Result<EgresoResponse>.Success(Map(salario));
    }

    public async Task<Result<EgresoResponse>> RegistrarPagoAsync(int id, RegistrarPagoRequest request, int userId)
    {
        var egreso = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id);
        if (egreso is null)
            return Result<EgresoResponse>.Failure("Egreso no encontrado.", ErrorType.NotFound);

        if (!Enum.TryParse<MetodoPago>(request.MetodoPago, ignoreCase: true, out var metodo))
            return Result<EgresoResponse>.Failure("Método de pago inválido.", ErrorType.Validation);

        if (!DateOnly.TryParse(request.FechaPago, out var fechaPago))
            return Result<EgresoResponse>.Failure("Fecha de pago inválida.", ErrorType.Validation);

        try { egreso.RegistrarPago(metodo, fechaPago, request.NroComprobante); }
        catch (InvalidOperationException ex)
        { return Result<EgresoResponse>.Failure(ex.Message, ErrorType.Conflict); }

        var pago = new EgresoPago
        {
            EgresoId              = egreso.Id,
            FechaPago             = fechaPago,
            MetodoPago            = metodo,
            NumeroComprobante     = request.NroComprobante?.Trim(),
            Observaciones         = request.Observaciones?.Trim(),
            RegistradoPorUserId   = userId,
        };
        db.EgresosPagos.Add(pago);

        var movimiento = new MovimientoCaja
        {
            SucursalId = egreso.SucursalId,
            Tipo       = TipoMovimientoCaja.Egreso,
            Monto      = egreso.Monto,
            Concepto   = $"Pago egreso #{egreso.Id}: {egreso.Concepto}",
            MetodoPago = metodo,
            EgresoId   = egreso.Id,
            Fecha      = fechaPago,
            Referencia = request.NroComprobante?.Trim(),
            CreatedAt  = DateTime.UtcNow,
        };
        db.MovimientosCaja.Add(movimiento);

        await db.SaveChangesAsync();

        pago.RegistradoPorUser = await db.Users.Include(u => u.Person).FirstOrDefaultAsync(u => u.Id == userId);
        return Result<EgresoResponse>.Success(Map(egreso, pago));
    }

    public async Task<Result<EgresoResponse>> AprobarEgresoAsync(int id, int userId)
    {
        var egreso = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id);
        if (egreso is null)
            return Result<EgresoResponse>.Failure("Egreso no encontrado.", ErrorType.NotFound);

        try { egreso.Aprobar(userId); }
        catch (InvalidOperationException ex)
        { return Result<EgresoResponse>.Failure(ex.Message, ErrorType.Conflict); }

        await db.SaveChangesAsync();
        return Result<EgresoResponse>.Success(Map(egreso));
    }

    public async Task<Result<EgresoResponse>> RechazarEgresoAsync(int id, RechazarEgresoRequest request)
    {
        var egreso = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id);
        if (egreso is null)
            return Result<EgresoResponse>.Failure("Egreso no encontrado.", ErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(request.Motivo))
            return Result<EgresoResponse>.Failure("El motivo de rechazo es obligatorio.", ErrorType.Validation);

        try { egreso.Rechazar(request.Motivo.Trim()); }
        catch (InvalidOperationException ex)
        { return Result<EgresoResponse>.Failure(ex.Message, ErrorType.Conflict); }

        await db.SaveChangesAsync();
        return Result<EgresoResponse>.Success(Map(egreso));
    }

    public async Task<Result<EgresoResponse>> AnularEgresoAsync(int id, AnularEgresoRequest request)
    {
        var egreso = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id);
        if (egreso is null)
            return Result<EgresoResponse>.Failure("Egreso no encontrado.", ErrorType.NotFound);

        if (egreso.Estado == EstadoEgreso.Anulado)
            return Result<EgresoResponse>.Failure("El egreso ya está anulado.", ErrorType.Conflict);

        if (egreso.Estado == EstadoEgreso.Pagado)
            return Result<EgresoResponse>.Failure("No se puede anular un egreso pagado.", ErrorType.Conflict);

        egreso.Estado    = EstadoEgreso.Anulado;
        egreso.UpdatedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Motivo))
            egreso.Observaciones = (egreso.Observaciones is not null
                ? egreso.Observaciones + " | " : "") + $"Anulado: {request.Motivo.Trim()}";

        await db.SaveChangesAsync();
        return Result<EgresoResponse>.Success(Map(egreso));
    }

    public async Task<Result<EgresoResponse>> GetEgresoByIdAsync(int id)
    {
        var egreso = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id);
        if (egreso is null)
            return Result<EgresoResponse>.Failure("Egreso no encontrado.", ErrorType.NotFound);

        var pago = await db.EgresosPagos
            .Include(p => p.RegistradoPorUser).ThenInclude(u => u!.Person)
            .FirstOrDefaultAsync(p => p.EgresoId == id);

        return Result<EgresoResponse>.Success(Map(egreso, pago));
    }

    public async Task<Result<PagedResult<EgresoResponse>>> GetEgresosAsync(
        string? tipo, string? estado, string? fechaDesde, string? fechaHasta,
        bool? soloVencidos, int page, int pageSize, Guid? sucursalId = null)
    {
        var query = BaseQuery();

        if (sucursalId.HasValue)
            query = query.Where(e => e.SucursalId == sucursalId.Value);

        if (!string.IsNullOrWhiteSpace(tipo) && Enum.TryParse<TipoEgreso>(tipo, ignoreCase: true, out var t))
            query = query.Where(e => e.Tipo == t);

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoEgreso>(estado, ignoreCase: true, out var s))
            query = query.Where(e => e.Estado == s);

        if (DateOnly.TryParse(fechaDesde, out var desde))
            query = query.Where(e => e.FechaEmision >= desde);

        if (DateOnly.TryParse(fechaHasta, out var hasta))
            query = query.Where(e => e.FechaEmision <= hasta);

        if (soloVencidos == true)
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            query = query.Where(e =>
                e.Estado != EstadoEgreso.Pagado &&
                e.Estado != EstadoEgreso.Anulado &&
                e.FechaVencimiento.HasValue &&
                e.FechaVencimiento.Value < hoy);
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<EgresoResponse>>.Success(new PagedResult<EgresoResponse>
        {
            Items      = items.Select(e => Map(e)),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
        });
    }
}