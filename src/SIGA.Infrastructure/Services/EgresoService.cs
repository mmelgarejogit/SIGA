using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Egresos;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class EgresoService(AppDbContext db, ICurrentUserContext current) : IEgresoService
{
    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static EgresoResponse Map(Egreso e)
    {
        var r = new EgresoResponse
        {
            Id               = e.Id,
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
            FechaAprobacion  = e.FechaAprobacion?.ToString("yyyy-MM-dd"),
            NroComprobante    = e.NroComprobante,
            PagoExterno       = e.PagoExterno,
            MotivoPagoExterno = e.MotivoPagoExterno,
            CreatedAt         = e.CreatedAt,
        };

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
            r.Periodo            = h.Periodo;
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
            r.Periodo        = s.Periodo;
        }
        else if (e is EgresoFacturaLaboratorio efl)
        {
            r.FacturaLaboratorioId = efl.FacturaLaboratorioId;
            r.NroFactura           = efl.FacturaLaboratorio?.NumeroFactura;
            r.ProveedorNombre      = efl.FacturaLaboratorio?.TrabajoPedido?.LaboratorioProveedor?.Nombre;
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
            .Include(e => (e as FacturaCompra)!.Proveedor)
            .Include(e => (e as Honorario)!.Professional)
                .ThenInclude(p => p!.User)
                    .ThenInclude(u => u!.Person)
            .Include(e => (e as GastoGeneral)!.CategoriaGasto)
            .Include(e => (e as SalarioEmpleado)!.Empleado)
                .ThenInclude(emp => emp!.User)
                    .ThenInclude(u => u!.Person)
            .Include(e => (e as EgresoFacturaLaboratorio)!.FacturaLaboratorio)
                .ThenInclude(fl => fl!.TrabajoPedido)
                    .ThenInclude(tp => tp!.LaboratorioProveedor);

    // ── Categorías ───────────────────────────────────────────────────────────────

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

    // ── Crear Egresos ─────────────────────────────────────────────────────────────

    public async Task<Result<EgresoResponse>> CrearFacturaCompraAsync(CrearFacturaCompraRequest request)
    {
        if (!DateOnly.TryParse(request.FechaEmision, out var fechaEmision))
            return Result<EgresoResponse>.Failure("Fecha de emisión inválida.", ErrorType.Validation);

        DateOnly? fechaVencimiento = null;
        if (!string.IsNullOrWhiteSpace(request.FechaVencimiento))
        {
            if (!DateOnly.TryParse(request.FechaVencimiento, out var fv))
                return Result<EgresoResponse>.Failure("Fecha de vencimiento inválida.", ErrorType.Validation);
            fechaVencimiento = fv;
        }

        if (!Enum.TryParse<CondicionVenta>(request.CondicionVenta, ignoreCase: true, out var condicion))
            return Result<EgresoResponse>.Failure("Condición de venta inválida. Valores válidos: Contado, Credito.", ErrorType.Validation);

        var proveedorExists = await db.Proveedores.AnyAsync(p => p.Id == request.ProveedorId);
        if (!proveedorExists)
            return Result<EgresoResponse>.Failure("Proveedor no encontrado.", ErrorType.NotFound);

        var factura = new FacturaCompra
        {
            SucursalId        = await SucursalResolver.WriteBranchAsync(db, current),
            RegistradoPorId   = current.UserId,
            ProveedorId       = request.ProveedorId,
            PedidoProveedorId = request.PedidoProveedorId,
            NroFactura        = request.NroFactura?.Trim(),
            MontoExento       = request.MontoExento,
            MontoGravado5     = request.MontoGravado5,
            MontoGravado10    = request.MontoGravado10,
            CondicionVenta    = condicion,
            Concepto          = request.Concepto.Trim(),
            Observaciones     = request.Observaciones?.Trim(),
            FechaEmision      = fechaEmision,
            FechaVencimiento  = fechaVencimiento,
            Estado            = EstadoEgreso.Aprobado,
        };
        factura.Monto = factura.MontoTotal;

        db.FacturasCompra.Add(factura);
        await db.SaveChangesAsync();

        await db.Entry(factura).Reference(f => f.Proveedor).LoadAsync();
        return Result<EgresoResponse>.Success(Map(factura));
    }

    public async Task<Result<EgresoResponse>> CrearHonorarioAsync(CrearHonorarioRequest request)
    {
        if (!DateOnly.TryParse(request.FechaEmision, out var fechaEmision))
            return Result<EgresoResponse>.Failure("Fecha de emisión inválida.", ErrorType.Validation);

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
            SucursalId       = await SucursalResolver.WriteBranchAsync(db, current),
            RegistradoPorId  = current.UserId,
            ProfessionalId   = request.ProfessionalId,
            Monto            = request.Monto,
            Concepto         = request.Concepto.Trim(),
            Periodo          = request.Periodo?.Trim(),
            Observaciones    = request.Observaciones?.Trim(),
            FechaEmision     = fechaEmision,
            FechaVencimiento = fechaVencimiento,
            Estado           = EstadoEgreso.Pendiente,
        };
        db.Honorarios.Add(honorario);
        await db.SaveChangesAsync();

        honorario.Professional = professional;
        return Result<EgresoResponse>.Success(Map(honorario));
    }

    public async Task<Result<EgresoResponse>> CrearGastoGeneralAsync(CrearGastoGeneralRequest request)
    {
        if (!DateOnly.TryParse(request.FechaEmision, out var fechaEmision))
            return Result<EgresoResponse>.Failure("Fecha de emisión inválida.", ErrorType.Validation);

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
            SucursalId       = await SucursalResolver.WriteBranchAsync(db, current),
            RegistradoPorId  = current.UserId,
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
        return Result<EgresoResponse>.Success(Map(gasto));
    }

    public async Task<Result<EgresoResponse>> CrearSalarioAsync(CrearSalarioRequest request)
    {
        if (!DateOnly.TryParse(request.FechaEmision, out var fechaEmision))
            return Result<EgresoResponse>.Failure("Fecha de emisión inválida.", ErrorType.Validation);

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
            SucursalId       = await SucursalResolver.WriteBranchAsync(db, current),
            RegistradoPorId  = current.UserId,
            EmpleadoId       = request.EmpleadoId,
            Monto            = request.Monto,
            Concepto         = request.Concepto.Trim(),
            Periodo          = request.Periodo?.Trim(),
            Observaciones    = request.Observaciones?.Trim(),
            FechaEmision     = fechaEmision,
            FechaVencimiento = fechaVencimiento,
            Estado           = EstadoEgreso.Pendiente,
        };
        db.SalariosEmpleado.Add(salario);
        await db.SaveChangesAsync();

        salario.Empleado = empleado;
        return Result<EgresoResponse>.Success(Map(salario));
    }

    // ── Transiciones de estado ────────────────────────────────────────────────────

    public async Task<Result<EgresoResponse>> RegistrarPagoAsync(int id, RegistrarPagoRequest request, int userId)
    {
        var egreso = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id);
        if (egreso is null)
            return Result<EgresoResponse>.Failure("Egreso no encontrado.", ErrorType.NotFound);

        if (!Enum.TryParse<MetodoPago>(request.MetodoPago, ignoreCase: true, out var metodo))
            return Result<EgresoResponse>.Failure("Método de pago inválido.", ErrorType.Validation);

        if (!DateOnly.TryParse(request.FechaPago, out var fechaPago))
            return Result<EgresoResponse>.Failure("Fecha de pago inválida.", ErrorType.Validation);

        if (!string.IsNullOrWhiteSpace(request.Observaciones))
            egreso.Observaciones = (egreso.Observaciones is not null
                ? egreso.Observaciones + " | " : "") + request.Observaciones.Trim();

        try
        {
            egreso.RegistrarPago(metodo, fechaPago, request.NroComprobante);
        }
        catch (InvalidOperationException ex)
        {
            return Result<EgresoResponse>.Failure(ex.Message, ErrorType.Conflict);
        }

        if (request.EsExterno)
        {
            egreso.PagoExterno      = true;
            egreso.MotivoPagoExterno = request.MotivoExterno?.Trim();
        }
        else
        {
            var branch = await SucursalResolver.WriteBranchAsync(db, current);
            var sesion = await db.SesionesCaja.FirstOrDefaultAsync(s => s.Estado == EstadoSesionCaja.Abierta && s.SucursalId == branch);
            if (sesion == null)
                return Result<EgresoResponse>.Failure("No hay una caja abierta en tu sucursal. Abrí la caja antes de registrar pagos.", ErrorType.Conflict);

            db.MovimientosCaja.Add(new MovimientoCaja
            {
                Tipo            = TipoMovimientoCaja.Egreso,
                Monto           = egreso.Monto,
                Concepto        = $"Pago egreso — {egreso.Concepto}",
                MetodoPago      = metodo,
                SucursalId      = sesion.SucursalId,
                EgresoId        = egreso.Id,
                SesionCajaId    = sesion.Id,
                RegistradoPorId = userId,
                Fecha           = fechaPago,
                Referencia      = request.NroComprobante,
                CreatedAt       = DateTime.UtcNow,
            });
        }

        await db.SaveChangesAsync();
        return Result<EgresoResponse>.Success(Map(egreso));
    }

    public async Task<Result<EgresoResponse>> AprobarEgresoAsync(int id)
    {
        var egreso = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id);
        if (egreso is null)
            return Result<EgresoResponse>.Failure("Egreso no encontrado.", ErrorType.NotFound);

        try { egreso.Aprobar(); }
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

    // ── Consultas ─────────────────────────────────────────────────────────────────

    public async Task<Result<EgresoResponse>> GetEgresoByIdAsync(int id)
    {
        var egreso = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id);
        if (egreso is null)
            return Result<EgresoResponse>.Failure("Egreso no encontrado.", ErrorType.NotFound);
        return Result<EgresoResponse>.Success(Map(egreso));
    }

    public async Task<Result<PagedResult<EgresoResponse>>> GetEgresosAsync(
        string? tipo, string? estado, string? fechaDesde, string? fechaHasta,
        bool? soloVencidos, int page, int pageSize)
    {
        var query = BaseQuery();

        if (current.SucursalId is int b)
            query = query.Where(e => e.SucursalId == b);

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
            Items      = items.Select(Map),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
        });
    }
}
