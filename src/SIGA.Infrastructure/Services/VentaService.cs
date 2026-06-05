using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class VentaService(AppDbContext db) : IVentaService
{
    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static VentaDto Map(Venta v) => new()
    {
        Id                = v.Id,
        NumeroComprobante = v.NumeroComprobante,
        PatientId         = v.PatientId,
        PacienteNombre    = $"{v.Patient?.User?.Person?.FirstName} {v.Patient?.User?.Person?.LastName}".Trim(),
        RecetaId          = v.RecetaId,
        Estado            = v.Estado.ToString(),
        Tipo              = v.Tipo.ToString(),
        CondicionVenta    = v.CondicionVenta.ToString(),
        FechaVenta        = v.FechaVenta.ToString("yyyy-MM-dd"),
        FechaConfirmacion = v.FechaConfirmacion?.ToString("yyyy-MM-dd"),
        FechaComprobante  = v.FechaComprobante?.ToString("yyyy-MM-dd"),
        MontoExento       = v.MontoExento,
        MontoGravado5     = v.MontoGravado5,
        MontoGravado10    = v.MontoGravado10,
        Total             = v.Total,
        MontoSeña         = v.MontoSeña,
        TotalCobrado      = v.TotalCobrado,
        SaldoPendiente    = v.SaldoPendiente,
        Observaciones     = v.Observaciones,
        CreatedAt         = v.CreatedAt,
        Lineas = v.Lineas.Select(l => new VentaLineaDto
        {
            Id              = l.Id,
            Tipo            = l.Tipo.ToString(),
            ProductoId      = l.ProductoId,
            ServicioId      = l.ServicioId,
            Descripcion     = l.Descripcion,
            Cantidad        = l.Cantidad,
            PrecioUnitario  = l.PrecioUnitario,
            Descuento       = l.Descuento,
            CategoriaFiscal = l.CategoriaFiscal.ToString(),
            Subtotal        = l.Subtotal,
        }).ToList(),
        Cobros = v.Cobros.Select(c => new CobroDto
        {
            Id         = c.Id,
            Tipo       = c.Tipo.ToString(),
            MontoTotal = c.MontoTotal,
            Fecha      = c.Fecha.ToString("yyyy-MM-dd"),
            Anulado    = c.Anulado,
            Lineas     = c.Lineas.Select(l => new CobroLineaDto
            {
                Id         = l.Id,
                MetodoPago = l.MetodoPago.ToString(),
                Monto      = l.Monto,
            }).ToList(),
            CreatedAt  = c.CreatedAt,
        }).ToList(),
        Factura = v.Factura == null ? null : new FacturaVentaDto
        {
            Id              = v.Factura.Id,
            NumeroFactura   = v.Factura.NumeroFactura,
            Timbrado        = v.Factura.Timbrado,
            Establecimiento = v.Factura.Establecimiento,
            MontoExento     = v.Factura.MontoExento,
            MontoGravado5   = v.Factura.MontoGravado5,
            MontoGravado10  = v.Factura.MontoGravado10,
            Iva5            = v.Factura.Iva5,
            Iva10           = v.Factura.Iva10,
            Total           = v.Factura.Total,
            FechaEmision    = v.Factura.FechaEmision.ToString("yyyy-MM-dd"),
            Observaciones   = v.Factura.Observaciones,
        },
        Comprobante = v.Comprobante == null ? null : new ComprobanteDto
        {
            Id              = v.Comprobante.Id,
            Tipo            = v.Comprobante.Tipo.ToString(),
            Estado          = v.Comprobante.Estado.ToString(),
            MotivoAnulacion = v.Comprobante.MotivoAnulacion,
            FechaEmision    = v.Comprobante.FechaEmision.ToString("o"),
            FechaAnulacion  = v.Comprobante.FechaAnulacion?.ToString("o"),
        },
        Devoluciones = v.Devoluciones.Select(MapDevolucion).ToList(),
        TrabajoPedido = v.TrabajoPedido == null ? null : MapTrabajoPedidoDto(v.TrabajoPedido),
    };

    private IQueryable<Venta> BaseQuery() =>
        db.Ventas
            .Include(v => v.Patient).ThenInclude(p => p.User).ThenInclude(u => u!.Person)
            .Include(v => v.Lineas).ThenInclude(l => l.Producto)
            .Include(v => v.Lineas).ThenInclude(l => l.Servicio)
            .Include(v => v.Cobros).ThenInclude(c => c.Lineas)
            .Include(v => v.Factura)
            .Include(v => v.Comprobante)
            .Include(v => v.TrabajoPedido).ThenInclude(tp => tp!.TipoLente)
            .Include(v => v.TrabajoPedido).ThenInclude(tp => tp!.Tratamientos)
            .Include(v => v.TrabajoPedido).ThenInclude(tp => tp!.LaboratorioProveedor)
            .Include(v => v.TrabajoPedido).ThenInclude(tp => tp!.ArmazonProducto)
            .Include(v => v.TrabajoPedido).ThenInclude(tp => tp!.Factura).ThenInclude(f => f!.EmitidoPor).ThenInclude(u => u.Person)
            .Include(v => v.Devoluciones).ThenInclude(d => d.Lineas).ThenInclude(l => l.ProductoDevuelto)
            .Include(v => v.Devoluciones).ThenInclude(d => d.Lineas).ThenInclude(l => l.ProductoNuevo)
            .Include(v => v.Devoluciones).ThenInclude(d => d.SolicitadoPor).ThenInclude(u => u.Person)
            .Include(v => v.Devoluciones).ThenInclude(d => d.ConfirmadoPor!);

    // ── Queries ──────────────────────────────────────────────────────────────────

    public async Task<Result<VentaDto>> GetVentaByIdAsync(int id)
    {
        var venta = await BaseQuery().FirstOrDefaultAsync(v => v.Id == id);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);
        return Result<VentaDto>.Success(Map(venta));
    }

    public async Task<Result<PagedResult<VentaDto>>> GetVentasAsync(
        string? estado, string? tipo, string? fechaDesde, string? fechaHasta,
        int? patientId, int page, int pageSize)
    {
        var query = BaseQuery();

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoVenta>(estado, out var e))
            query = query.Where(v => v.Estado == e);

        if (!string.IsNullOrWhiteSpace(tipo) && Enum.TryParse<TipoVenta>(tipo, out var t))
            query = query.Where(v => v.Tipo == t);

        if (!string.IsNullOrWhiteSpace(fechaDesde) && DateOnly.TryParse(fechaDesde, out var desde))
            query = query.Where(v => v.FechaVenta >= desde);

        if (!string.IsNullOrWhiteSpace(fechaHasta) && DateOnly.TryParse(fechaHasta, out var hasta))
            query = query.Where(v => v.FechaVenta <= hasta);

        if (patientId.HasValue)
            query = query.Where(v => v.PatientId == patientId.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(v => v.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<VentaDto>>.Success(new PagedResult<VentaDto>
        {
            Items      = items.Select(Map).ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        });
    }

    // ── Commands ─────────────────────────────────────────────────────────────────

    public async Task<Result<VentaDto>> CrearVentaAsync(CrearVentaRequest request)
    {
        if (!request.Lineas.Any())
            return Result<VentaDto>.Failure("La venta debe tener al menos una línea", ErrorType.Validation);

        var condicion = Enum.TryParse<CondicionVenta>(request.CondicionVenta, out var c) ? c : CondicionVenta.Contado;
        var tipo      = Enum.TryParse<TipoVenta>(request.Tipo ?? "Directa", out var tv) ? tv : TipoVenta.Directa;

        var venta = new Venta
        {
            NumeroComprobante = "",
            PatientId         = request.PatientId,
            RecetaId          = request.RecetaId,
            CondicionVenta    = condicion,
            Tipo              = tipo,
            FechaVenta        = DateOnly.Parse(request.FechaVenta),
            Observaciones     = request.Observaciones,
            Estado            = EstadoVenta.Borrador,
            CreatedAt         = DateTime.UtcNow,
            UpdatedAt         = DateTime.UtcNow,
        };

        foreach (var lr in request.Lineas)
        {
            var tipoLinea = Enum.TryParse<TipoLineaVenta>(lr.Tipo, out var tl) ? tl : TipoLineaVenta.Producto;
            var cat       = Enum.TryParse<CategoriaFiscal>(lr.CategoriaFiscal, out var cf) ? cf : CategoriaFiscal.Gravado10;

            string descripcion = lr.Descripcion ?? "";
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                if (tipoLinea == TipoLineaVenta.Producto && lr.ProductoId.HasValue)
                {
                    var prod = await db.Productos.FindAsync(lr.ProductoId.Value);
                    descripcion = prod?.Nombre ?? "Producto";
                }
                else if (tipoLinea == TipoLineaVenta.Servicio && lr.ServicioId.HasValue)
                {
                    var serv = await db.Servicios.FindAsync(lr.ServicioId.Value);
                    descripcion = serv?.Nombre ?? "Servicio";
                }
            }

            venta.Lineas.Add(new VentaLinea
            {
                Tipo            = tipoLinea,
                ProductoId      = lr.ProductoId,
                ServicioId      = lr.ServicioId,
                Descripcion     = descripcion,
                Cantidad        = lr.Cantidad,
                PrecioUnitario  = lr.PrecioUnitario,
                Descuento       = lr.Descuento,
                CategoriaFiscal = cat,
            });
        }

        db.Ventas.Add(venta);
        await db.SaveChangesAsync();

        venta.NumeroComprobante = $"REC-{venta.Id:D7}";
        await db.SaveChangesAsync();

        return await GetVentaByIdAsync(venta.Id);
    }

    public async Task<Result<VentaDto>> ConfirmarVentaAsync(int id, int userId)
    {
        var venta = await db.Ventas.FindAsync(id);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);
        if (!venta.PuedeConfirmarse())
            return Result<VentaDto>.Failure("Solo se pueden confirmar ventas en estado Borrador", ErrorType.Conflict);

        venta.Estado            = venta.Tipo == TipoVenta.Directa
            ? EstadoVenta.ListaParaCobrar
            : EstadoVenta.EnProceso;
        venta.FechaConfirmacion = DateOnly.FromDateTime(DateTime.UtcNow);
        venta.UpdatedAt         = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return await GetVentaByIdAsync(id);
    }

    public async Task<Result<VentaDto>> CancelarVentaAsync(int id, CancelarVentaRequest request)
    {
        var venta = await db.Ventas.FindAsync(id);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);
        if (!venta.PuedeCancelarse())
            return Result<VentaDto>.Failure(
                "No se puede cancelar una venta en el estado actual", ErrorType.Conflict);

        venta.Estado      = EstadoVenta.Cancelada;
        venta.UpdatedAt   = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(request.Motivo))
            venta.Observaciones = string.IsNullOrWhiteSpace(venta.Observaciones)
                ? $"Cancelada: {request.Motivo}"
                : $"{venta.Observaciones} | Cancelada: {request.Motivo}";

        await db.SaveChangesAsync();
        return await GetVentaByIdAsync(id);
    }

    public async Task<Result<bool>> EliminarPresupuestoAsync(int id)
    {
        var venta = await db.Ventas.FindAsync(id);
        if (venta == null) return Result<bool>.Failure("Venta no encontrada.", ErrorType.NotFound);
        if (venta.Estado != EstadoVenta.Borrador)
            return Result<bool>.Failure("Solo se pueden eliminar presupuestos en estado Borrador.", ErrorType.Conflict);

        db.Ventas.Remove(venta);
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    public async Task<Result<VentaDto>> RegistrarCobroAsync(RegistrarCobroRequest request, int userId)
    {
        if (!request.Lineas.Any())
            return Result<VentaDto>.Failure("El cobro debe tener al menos un método de pago", ErrorType.Validation);

        var venta = await BaseQuery().FirstOrDefaultAsync(v => v.Id == request.VentaId);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);

        if (venta.Estado == EstadoVenta.Cancelada)
            return Result<VentaDto>.Failure("No se puede cobrar una venta cancelada", ErrorType.Conflict);

        if (venta.Estado == EstadoVenta.ComprobanteEmitido)
            return Result<VentaDto>.Failure("La venta ya tiene comprobante emitido", ErrorType.Conflict);

        if (!Enum.TryParse<TipoCobro>(request.Tipo, out var tipoCobro))
            return Result<VentaDto>.Failure("Tipo de cobro inválido", ErrorType.Validation);

        var lineas = new List<(MetodoPago metodo, decimal monto)>();
        foreach (var l in request.Lineas)
        {
            if (!Enum.TryParse<MetodoPago>(l.MetodoPago, out var metodo))
                return Result<VentaDto>.Failure($"Método de pago inválido: {l.MetodoPago}", ErrorType.Validation);
            if (l.Monto <= 0)
                return Result<VentaDto>.Failure("El monto de cada línea debe ser mayor a cero", ErrorType.Validation);
            lineas.Add((metodo, l.Monto));
        }

        var montoTotal = lineas.Sum(l => l.monto);
        var fecha      = DateOnly.TryParse(request.Fecha, out var f) ? f : DateOnly.FromDateTime(DateTime.UtcNow);

        var cobro = new Cobro
        {
            VentaId         = request.VentaId,
            Tipo            = tipoCobro,
            MontoTotal      = montoTotal,
            Fecha           = fecha,
            RegistradoPorId = userId,
            CreatedAt       = DateTime.UtcNow,
        };

        foreach (var (metodo, monto) in lineas)
        {
            cobro.Lineas.Add(new CobroLinea { MetodoPago = metodo, Monto = monto });

            db.MovimientosCaja.Add(new MovimientoCaja
            {
                Tipo       = TipoMovimientoCaja.Ingreso,
                Monto      = monto,
                Concepto   = $"Cobro {tipoCobro} — venta {venta.NumeroComprobante}",
                MetodoPago = metodo,
                VentaId    = venta.Id,
                Fecha      = fecha,
                CreatedAt  = DateTime.UtcNow,
            });
        }

        db.Cobros.Add(cobro);
        venta.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return await GetVentaByIdAsync(request.VentaId);
    }

    public async Task<Result<VentaDto>> EmitirComprobanteAsync(int ventaId, int userId)
    {
        var venta = await BaseQuery().FirstOrDefaultAsync(v => v.Id == ventaId);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);
        if (!venta.PuedeEmitirComprobante())
            return Result<VentaDto>.Failure(
                "Solo se puede emitir comprobante cuando la venta está lista para cobrar", ErrorType.Conflict);
        if (venta.Comprobante != null)
            return Result<VentaDto>.Failure("La venta ya tiene comprobante emitido", ErrorType.Conflict);

        var now = DateTime.UtcNow;

        // Egreso de stock por cada línea de producto
        foreach (var linea in venta.Lineas.Where(l => l.Tipo == TipoLineaVenta.Producto && l.ProductoId.HasValue))
        {
            db.MovimientosStock.Add(new MovimientoStock
            {
                ProductoId      = linea.ProductoId!.Value,
                Tipo            = "Salida",
                Cantidad        = linea.Cantidad,
                Motivo          = $"Comprobante venta {venta.NumeroComprobante}",
                FechaMovimiento = now,
                Estado          = "Aprobado",
                CreatedAt       = now,
            });
        }

        // Movimiento de caja solo para ventas CONTADO (crédito ya tiene sus cobros)
        if (venta.CondicionVenta == CondicionVenta.Contado)
        {
            db.MovimientosCaja.Add(new MovimientoCaja
            {
                Tipo       = TipoMovimientoCaja.Ingreso,
                Monto      = venta.Total,
                Concepto   = $"Comprobante venta {venta.NumeroComprobante}",
                MetodoPago = MetodoPago.Efectivo,
                VentaId    = venta.Id,
                Fecha      = DateOnly.FromDateTime(now),
                CreatedAt  = now,
            });
        }

        venta.Comprobante = new Comprobante
        {
            Tipo          = TipoComprobante.ReciboSimple,
            Estado        = EstadoComprobante.Emitido,
            EmitidoPorId  = userId,
            FechaEmision  = now,
            CreatedAt     = now,
        };

        venta.Estado           = EstadoVenta.ComprobanteEmitido;
        venta.FechaComprobante = DateOnly.FromDateTime(now);
        venta.UpdatedAt        = now;

        await db.SaveChangesAsync();
        return await GetVentaByIdAsync(ventaId);
    }

    public async Task<Result<VentaDto>> EmitirFacturaAsync(EmitirFacturaRequest request)
    {
        var venta = await BaseQuery().FirstOrDefaultAsync(v => v.Id == request.VentaId);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);
        if (venta.Factura != null) return Result<VentaDto>.Failure("La venta ya tiene factura emitida", ErrorType.Conflict);

        db.FacturasVenta.Add(new FacturaVenta
        {
            VentaId         = request.VentaId,
            NumeroFactura   = request.NumeroFactura,
            Timbrado        = request.Timbrado,
            Establecimiento = request.Establecimiento,
            MontoExento     = venta.MontoExento,
            MontoGravado5   = venta.MontoGravado5,
            MontoGravado10  = venta.MontoGravado10,
            FechaEmision    = DateOnly.Parse(request.FechaEmision),
            Observaciones   = request.Observaciones,
            CreatedAt       = DateTime.UtcNow,
        });

        await db.SaveChangesAsync();
        return await GetVentaByIdAsync(request.VentaId);
    }

    private static TrabajoPedidoDto MapTrabajoPedidoDto(TrabajoPedido tp) => new()
    {
        Id                     = tp.Id,
        RecetaId               = tp.RecetaId,
        TipoLenteId            = tp.TipoLenteId,
        TipoLenteNombre        = tp.TipoLente.Nombre,
        Tratamientos           = tp.Tratamientos.Select(t => new TrabajoPedidoTratamientoDto { Id = t.Id, Nombre = t.Nombre }).ToList(),
        ArmazonProductoId      = tp.ArmazonProductoId,
        ArmazonProductoNombre  = tp.ArmazonProducto?.Nombre,
        LaboratorioProveedorId = tp.LaboratorioProveedorId,
        LaboratorioNombre      = tp.LaboratorioProveedor.Nombre,
        Estado                 = tp.Estado.ToString(),
        FechaEnvio             = tp.FechaEnvio?.ToString("yyyy-MM-dd"),
        FechaRecepcion         = tp.FechaRecepcion?.ToString("yyyy-MM-dd"),
        Observacion            = tp.Observacion,
        CreatedAt              = tp.CreatedAt,
    };

    private static TrabajoPedidoListDto MapTrabajoPedidoList(TrabajoPedido tp) => new()
    {
        Id                    = tp.Id,
        VentaId               = tp.VentaId,
        NumeroComprobante     = tp.Venta?.NumeroComprobante ?? "",
        PacienteNombre        = $"{tp.Venta?.Patient?.User?.Person?.FirstName} {tp.Venta?.Patient?.User?.Person?.LastName}".Trim(),
        TipoLenteNombre       = tp.TipoLente.Nombre,
        Tratamientos          = tp.Tratamientos.Select(t => t.Nombre).ToList(),
        LaboratorioNombre     = tp.LaboratorioProveedor.Nombre,
        Estado                = tp.Estado.ToString(),
        ObservacionAprobacion = tp.ObservacionAprobacion,
        AprobadoPorNombre     = tp.AprobadoPor == null ? null : $"{tp.AprobadoPor.Person?.FirstName} {tp.AprobadoPor.Person?.LastName}".Trim(),
        FechaEnvio            = tp.FechaEnvio?.ToString("yyyy-MM-dd"),
        FechaRecepcion        = tp.FechaRecepcion?.ToString("yyyy-MM-dd"),
        Observacion           = tp.Observacion,
        Factura               = tp.Factura == null ? null : new FacturaLaboratorioDto
        {
            Id               = tp.Factura.Id,
            NumeroFactura    = tp.Factura.NumeroFactura,
            Timbrado         = tp.Factura.Timbrado,
            FechaEmision     = tp.Factura.FechaEmision.ToString("yyyy-MM-dd"),
            Monto            = tp.Factura.Monto,
            Observaciones    = tp.Factura.Observaciones,
            EmitidoPorNombre = $"{tp.Factura.EmitidoPor?.Person?.FirstName} {tp.Factura.EmitidoPor?.Person?.LastName}".Trim(),
            CreatedAt        = tp.Factura.CreatedAt,
        },
        CreatedAt = tp.CreatedAt,
    };

    private IQueryable<TrabajoPedido> TpBaseQuery() =>
        db.TrabajosPedido
            .Include(tp => tp.Venta).ThenInclude(v => v!.Patient).ThenInclude(p => p.User).ThenInclude(u => u!.Person)
            .Include(tp => tp.TipoLente)
            .Include(tp => tp.Tratamientos)
            .Include(tp => tp.LaboratorioProveedor)
            .Include(tp => tp.AprobadoPor).ThenInclude(u => u!.Person)
            .Include(tp => tp.Factura).ThenInclude(f => f!.EmitidoPor).ThenInclude(u => u.Person);

    private static DevolucionDto MapDevolucion(Devolucion d) => new()
    {
        Id                    = d.Id,
        VentaId               = d.VentaId,
        NumeroComprobante     = d.Venta?.NumeroComprobante ?? "",
        Tipo                  = d.Tipo.ToString(),
        Estado                = d.Estado.ToString(),
        Motivo                = d.Motivo,
        SolicitadoPorNombre   = $"{d.SolicitadoPor?.Person?.FirstName} {d.SolicitadoPor?.Person?.LastName}".Trim(),
        ConfirmadoPorNombre   = d.ConfirmadoPor == null ? null : $"{d.ConfirmadoPor.Person?.FirstName} {d.ConfirmadoPor.Person?.LastName}".Trim(),
        ObservacionesRevision = d.ObservacionesRevision,
        FechaRevision         = d.FechaRevision?.ToString("o"),
        Lineas = d.Lineas.Select(l => new DevolucionLineaDto
        {
            Id                     = l.Id,
            ProductoDevueltoId     = l.ProductoDevueltoId,
            ProductoDevueltoNombre = l.ProductoDevuelto?.Nombre ?? "",
            CantidadDevuelta       = l.CantidadDevuelta,
            ProductoNuevoId        = l.ProductoNuevoId,
            ProductoNuevoNombre    = l.ProductoNuevo?.Nombre,
            CantidadNueva          = l.CantidadNueva,
        }).ToList(),
        CreatedAt = d.CreatedAt,
    };

    public async Task<Result<List<VentaDto>>> GetCobrosPendientesAsync()
    {
        var ventas = await BaseQuery()
            .Where(v =>
                v.CondicionVenta == CondicionVenta.Credito &&
                v.Estado != EstadoVenta.Cancelada &&
                v.Estado != EstadoVenta.Borrador)
            .OrderBy(v => v.FechaVenta)
            .ToListAsync();

        // Filtrar en memoria las que tienen saldo pendiente real (requiere cobros cargados)
        var conSaldo = ventas.Where(v => v.SaldoPendiente > 0).Select(Map).ToList();
        return Result<List<VentaDto>>.Success(conSaldo);
    }

    // ── Trabajo a pedido ─────────────────────────────────────────────────────────

    public async Task<Result<VentaDto>> CrearTrabajoPedidoAsync(int ventaId, CrearTrabajoPedidoRequest request)
    {
        var venta = await BaseQuery().FirstOrDefaultAsync(v => v.Id == ventaId);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);

        if (venta.Tipo != TipoVenta.TrabajoAPedido)
            return Result<VentaDto>.Failure("Solo ventas de tipo Trabajo a Pedido pueden tener un pedido a laboratorio", ErrorType.Conflict);

        if (venta.Estado != EstadoVenta.EnProceso)
            return Result<VentaDto>.Failure("La venta debe estar en estado EnProceso para registrar el pedido al laboratorio", ErrorType.Conflict);

        if (venta.TrabajoPedido != null)
            return Result<VentaDto>.Failure("Esta venta ya tiene un pedido a laboratorio registrado", ErrorType.Conflict);

        var laboratorio = await db.Proveedores.FindAsync(request.LaboratorioProveedorId);
        if (laboratorio == null || !laboratorio.EsLaboratorio)
            return Result<VentaDto>.Failure("El proveedor no existe o no está marcado como laboratorio", ErrorType.Validation);

        var tipoLente = await db.TiposLente.FindAsync(request.TipoLenteId);
        if (tipoLente is null)
            return Result<VentaDto>.Failure("Tipo de lente no encontrado.", ErrorType.Validation);

        var tratamientos = request.TratamientoIds.Any()
            ? await db.Tratamientos.Where(t => request.TratamientoIds.Contains(t.Id)).ToListAsync()
            : [];

        var now = DateTime.UtcNow;
        var tp = new TrabajoPedido
        {
            VentaId                = ventaId,
            RecetaId               = request.RecetaId > 0 ? request.RecetaId : null,
            TipoLenteId            = request.TipoLenteId,
            ArmazonProductoId      = request.ArmazonProductoId,
            LaboratorioProveedorId = request.LaboratorioProveedorId,
            Observacion            = request.Observacion?.Trim(),
            Estado                 = EstadoTrabajoPedido.PendienteAprobacion,
            CreatedAt              = now,
            UpdatedAt              = now,
        };

        foreach (var t in tratamientos) tp.Tratamientos.Add(t);
        db.TrabajosPedido.Add(tp);

        await db.SaveChangesAsync();
        return await GetVentaByIdAsync(ventaId);
    }

    public async Task<Result<VentaDto>> RegistrarEnvioLabAsync(int ventaId, RegistrarEnvioLabRequest request)
    {
        var venta = await BaseQuery().FirstOrDefaultAsync(v => v.Id == ventaId);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);

        var tp = venta.TrabajoPedido;
        if (tp == null) return Result<VentaDto>.Failure("La venta no tiene pedido a laboratorio registrado", ErrorType.Conflict);

        if (tp.Estado != EstadoTrabajoPedido.PendienteEnvio)
            return Result<VentaDto>.Failure("El pedido ya fue enviado al laboratorio", ErrorType.Conflict);

        tp.Estado     = EstadoTrabajoPedido.Enviado;
        tp.FechaEnvio = DateOnly.FromDateTime(DateTime.UtcNow);
        if (!string.IsNullOrWhiteSpace(request.Observacion))
            tp.Observacion = request.Observacion.Trim();
        tp.UpdatedAt  = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return await GetVentaByIdAsync(ventaId);
    }

    public async Task<Result<VentaDto>> RegistrarRecepcionLabAsync(int ventaId, RegistrarRecepcionLabRequest request)
    {
        var venta = await BaseQuery().FirstOrDefaultAsync(v => v.Id == ventaId);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);

        var tp = venta.TrabajoPedido;
        if (tp == null) return Result<VentaDto>.Failure("La venta no tiene pedido a laboratorio registrado", ErrorType.Conflict);

        if (tp.Estado != EstadoTrabajoPedido.Enviado)
            return Result<VentaDto>.Failure("El pedido debe estar en estado Enviado para registrar la recepción", ErrorType.Conflict);

        var now = DateTime.UtcNow;
        tp.Estado          = EstadoTrabajoPedido.Recibido;
        tp.FechaRecepcion  = DateOnly.FromDateTime(now);
        if (!string.IsNullOrWhiteSpace(request.Observacion))
            tp.Observacion = request.Observacion.Trim();
        tp.UpdatedAt       = now;

        // Recepción del laboratorio → venta pasa a ListaParaCobrar
        venta.Estado    = EstadoVenta.ListaParaCobrar;
        venta.UpdatedAt = now;

        await db.SaveChangesAsync();
        return await GetVentaByIdAsync(ventaId);
    }

    // ── Trabajos a pedido — vistas globales ──────────────────────────────────────

    public async Task<Result<List<TrabajoPedidoListDto>>> GetTrabajosPedidoAsync(string? estado)
    {
        var query = TpBaseQuery();

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoTrabajoPedido>(estado, out var e))
            query = query.Where(tp => tp.Estado == e);

        var items = await query.OrderByDescending(tp => tp.CreatedAt).ToListAsync();
        return Result<List<TrabajoPedidoListDto>>.Success(items.Select(MapTrabajoPedidoList).ToList());
    }

    public async Task<Result<TrabajoPedidoListDto>> GestionarAprobacionAsync(
        int id, GestionarTrabajoPedidoRequest request, int userId, string userName)
    {
        if (request.Accion != "Aprobar" && request.Accion != "Rechazar")
            return Result<TrabajoPedidoListDto>.Failure("Acción inválida. Use 'Aprobar' o 'Rechazar'.", ErrorType.Validation);

        var tp = await TpBaseQuery().FirstOrDefaultAsync(t => t.Id == id);
        if (tp is null) return Result<TrabajoPedidoListDto>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (tp.Estado != EstadoTrabajoPedido.PendienteAprobacion)
            return Result<TrabajoPedidoListDto>.Failure("Solo se pueden gestionar pedidos en estado Pendiente de Aprobación.", ErrorType.Conflict);

        tp.AprobadoPorId        = userId;
        tp.ObservacionAprobacion = request.Observacion?.Trim();
        tp.Estado               = request.Accion == "Aprobar" ? EstadoTrabajoPedido.PendienteEnvio : EstadoTrabajoPedido.Rechazado;
        tp.UpdatedAt            = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Result<TrabajoPedidoListDto>.Success(MapTrabajoPedidoList(tp));
    }

    public async Task<Result<TrabajoPedidoListDto>> RegistrarEnvioLabAsync(int id)
    {
        var tp = await TpBaseQuery().FirstOrDefaultAsync(t => t.Id == id);
        if (tp is null) return Result<TrabajoPedidoListDto>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (tp.Estado != EstadoTrabajoPedido.PendienteEnvio)
            return Result<TrabajoPedidoListDto>.Failure("El pedido no está aprobado para envío.", ErrorType.Conflict);

        tp.Estado     = EstadoTrabajoPedido.Enviado;
        tp.FechaEnvio = DateOnly.FromDateTime(DateTime.UtcNow);
        tp.UpdatedAt  = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Result<TrabajoPedidoListDto>.Success(MapTrabajoPedidoList(tp));
    }

    public async Task<Result<TrabajoPedidoListDto>> RegistrarRecepcionLabAsync(int id)
    {
        var tp = await TpBaseQuery().FirstOrDefaultAsync(t => t.Id == id);
        if (tp is null) return Result<TrabajoPedidoListDto>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (tp.Estado != EstadoTrabajoPedido.Enviado)
            return Result<TrabajoPedidoListDto>.Failure("El pedido debe estar en estado Enviado.", ErrorType.Conflict);

        var now = DateTime.UtcNow;
        tp.Estado         = EstadoTrabajoPedido.Recibido;
        tp.FechaRecepcion = DateOnly.FromDateTime(now);
        tp.UpdatedAt      = now;

        // Disparar transición de venta
        var venta = await db.Ventas.FindAsync(tp.VentaId);
        if (venta != null)
        {
            venta.Estado    = EstadoVenta.ListaParaCobrar;
            venta.UpdatedAt = now;
        }

        await db.SaveChangesAsync();
        return Result<TrabajoPedidoListDto>.Success(MapTrabajoPedidoList(tp));
    }

    public async Task<Result<TrabajoPedidoListDto>> EmitirFacturaLaboratorioAsync(
        int id, EmitirFacturaLaboratorioRequest request, int userId)
    {
        var tp = await TpBaseQuery().FirstOrDefaultAsync(t => t.Id == id);
        if (tp is null) return Result<TrabajoPedidoListDto>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (tp.Estado != EstadoTrabajoPedido.Recibido)
            return Result<TrabajoPedidoListDto>.Failure("Solo se puede emitir factura de pedidos recibidos.", ErrorType.Conflict);

        if (tp.Factura != null)
            return Result<TrabajoPedidoListDto>.Failure("Este pedido ya tiene una factura registrada.", ErrorType.Conflict);

        db.FacturasLaboratorio.Add(new FacturaLaboratorio
        {
            TrabajoPedidoId = id,
            NumeroFactura   = request.NumeroFactura.Trim(),
            Timbrado        = request.Timbrado?.Trim(),
            FechaEmision    = DateOnly.Parse(request.FechaEmision),
            Monto           = request.Monto,
            Observaciones   = request.Observaciones?.Trim(),
            EmitidoPorId    = userId,
            CreatedAt       = DateTime.UtcNow,
        });

        await db.SaveChangesAsync();

        // Recargar para tener la factura con navegación
        tp = await TpBaseQuery().FirstAsync(t => t.Id == id);
        return Result<TrabajoPedidoListDto>.Success(MapTrabajoPedidoList(tp));
    }

    // ── Devoluciones ─────────────────────────────────────────────────────────────

    public async Task<Result<DevolucionDto>> SolicitarDevolucionAsync(
        int ventaId, SolicitarDevolucionRequest request, int userId, string userName)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo))
            return Result<DevolucionDto>.Failure("El motivo es obligatorio", ErrorType.Validation);

        if (!request.Lineas.Any())
            return Result<DevolucionDto>.Failure("Debe incluir al menos un producto a devolver", ErrorType.Validation);

        var venta = await BaseQuery().FirstOrDefaultAsync(v => v.Id == ventaId);
        if (venta == null) return Result<DevolucionDto>.Failure("Venta no encontrada", ErrorType.NotFound);

        if (!venta.PuedeDevolver())
            return Result<DevolucionDto>.Failure(
                "Solo se pueden registrar devoluciones de ventas con comprobante emitido", ErrorType.Conflict);

        if (!Enum.TryParse<TipoDevolucion>(request.Tipo, out var tipo))
            return Result<DevolucionDto>.Failure("Tipo de devolución inválido. Use 'Devolucion' o 'Cambio'", ErrorType.Validation);

        // Validar líneas
        foreach (var l in request.Lineas)
        {
            if (l.CantidadDevuelta <= 0)
                return Result<DevolucionDto>.Failure("La cantidad devuelta debe ser mayor a cero", ErrorType.Validation);

            if (tipo == TipoDevolucion.Cambio && (!l.ProductoNuevoId.HasValue || (l.CantidadNueva ?? 0) <= 0))
                return Result<DevolucionDto>.Failure(
                    "Para cambios, cada línea debe tener producto nuevo y cantidad nueva", ErrorType.Validation);
        }

        var now = DateTime.UtcNow;
        var devolucion = new Devolucion
        {
            VentaId         = ventaId,
            Tipo            = tipo,
            Estado          = EstadoDevolucion.Pendiente,
            Motivo          = request.Motivo.Trim(),
            SolicitadoPorId = userId,
            CreatedAt       = now,
        };

        foreach (var l in request.Lineas)
        {
            devolucion.Lineas.Add(new DevolucionLinea
            {
                ProductoDevueltoId = l.ProductoDevueltoId,
                CantidadDevuelta   = l.CantidadDevuelta,
                ProductoNuevoId    = l.ProductoNuevoId,
                CantidadNueva      = l.CantidadNueva,
            });
        }

        db.Devoluciones.Add(devolucion);
        await db.SaveChangesAsync();

        var saved = await db.Devoluciones
            .Include(d => d.Lineas).ThenInclude(l => l.ProductoDevuelto)
            .Include(d => d.Lineas).ThenInclude(l => l.ProductoNuevo)
            .Include(d => d.SolicitadoPor).ThenInclude(u => u.Person)
            .FirstAsync(d => d.Id == devolucion.Id);

        return Result<DevolucionDto>.Success(MapDevolucion(saved));
    }

    public async Task<Result<List<DevolucionDto>>> GetDevolucionesAsync(int ventaId)
    {
        var devoluciones = await db.Devoluciones
            .Include(d => d.Lineas).ThenInclude(l => l.ProductoDevuelto)
            .Include(d => d.Lineas).ThenInclude(l => l.ProductoNuevo)
            .Include(d => d.SolicitadoPor).ThenInclude(u => u.Person)
            .Include(d => d.ConfirmadoPor!)
            .Where(d => d.VentaId == ventaId)
            .OrderByDescending(d => d.CreatedAt)
            .ToListAsync();

        return Result<List<DevolucionDto>>.Success(devoluciones.Select(MapDevolucion).ToList());
    }

    public async Task<Result<DevolucionDto>> GestionarDevolucionAsync(
        int devolucionId, GestionarDevolucionRequest request, int userId, string userName)
    {
        if (request.Accion != "Confirmar" && request.Accion != "Rechazar")
            return Result<DevolucionDto>.Failure("Acción inválida. Use 'Confirmar' o 'Rechazar'", ErrorType.Validation);

        var devolucion = await db.Devoluciones
            .Include(d => d.Lineas).ThenInclude(l => l.ProductoDevuelto)
            .Include(d => d.Lineas).ThenInclude(l => l.ProductoNuevo)
            .Include(d => d.SolicitadoPor).ThenInclude(u => u.Person)
            .Include(d => d.Venta)
            .FirstOrDefaultAsync(d => d.Id == devolucionId);

        if (devolucion == null)
            return Result<DevolucionDto>.Failure("Devolución no encontrada", ErrorType.NotFound);

        if (devolucion.Estado != EstadoDevolucion.Pendiente)
            return Result<DevolucionDto>.Failure("Solo se pueden gestionar devoluciones en estado Pendiente", ErrorType.Conflict);

        var now = DateTime.UtcNow;
        devolucion.ConfirmadoPorId       = userId;
        devolucion.ObservacionesRevision = request.ObservacionesRevision?.Trim();
        devolucion.FechaRevision         = now;

        if (request.Accion == "Rechazar")
        {
            devolucion.Estado = EstadoDevolucion.Rechazada;
            await db.SaveChangesAsync();
            return Result<DevolucionDto>.Success(MapDevolucion(devolucion));
        }

        // Confirmar — transacción atómica de stock y caja
        devolucion.Estado = EstadoDevolucion.Confirmada;
        var fecha = DateOnly.FromDateTime(now);

        foreach (var linea in devolucion.Lineas)
        {
            // INGRESO stock del producto devuelto
            db.MovimientosStock.Add(new MovimientoStock
            {
                ProductoId      = linea.ProductoDevueltoId,
                Tipo            = "Entrada",
                Cantidad        = linea.CantidadDevuelta,
                Motivo          = $"Devolución #{devolucion.Id} — venta {devolucion.Venta.NumeroComprobante}",
                FechaMovimiento = now,
                Estado          = "Aprobado",
                CreatedAt       = now,
            });

            // EGRESO stock del producto nuevo (solo para Cambio)
            if (devolucion.Tipo == TipoDevolucion.Cambio && linea.ProductoNuevoId.HasValue)
            {
                db.MovimientosStock.Add(new MovimientoStock
                {
                    ProductoId      = linea.ProductoNuevoId.Value,
                    Tipo            = "Salida",
                    Cantidad        = linea.CantidadNueva ?? 0,
                    Motivo          = $"Cambio #{devolucion.Id} — venta {devolucion.Venta.NumeroComprobante}",
                    FechaMovimiento = now,
                    Estado          = "Aprobado",
                    CreatedAt       = now,
                });
            }
        }

        // Movimiento de caja negativo solo para Devolucion (no para Cambio — precio pendiente de definir)
        if (devolucion.Tipo == TipoDevolucion.Devolucion)
        {
            var totalDevuelto = devolucion.Lineas.Sum(l =>
            {
                var precio = l.ProductoDevuelto?.PrecioVenta ?? 0;
                return precio * l.CantidadDevuelta;
            });

            if (totalDevuelto > 0)
            {
                db.MovimientosCaja.Add(new MovimientoCaja
                {
                    Tipo       = TipoMovimientoCaja.Egreso,
                    Monto      = totalDevuelto,
                    Concepto   = $"Devolución #{devolucion.Id} — venta {devolucion.Venta.NumeroComprobante}",
                    MetodoPago = MetodoPago.Efectivo,
                    VentaId    = devolucion.VentaId,
                    Fecha      = fecha,
                    CreatedAt  = now,
                });
            }
        }

        await db.SaveChangesAsync();
        return Result<DevolucionDto>.Success(MapDevolucion(devolucion));
    }
}
