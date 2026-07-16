using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class VentaService(AppDbContext db, ICurrentUserContext current) : IVentaService
{
    // ── Helpers ──────────────────────────────────────────────────────────────────

    private static VentaDto Map(Venta v) => new()
    {
        Id                = v.Id,
        NumeroComprobante = v.NumeroComprobante,
        SucursalId        = v.SucursalId,
        SucursalNombre    = v.Sucursal?.Nombre,
        ClienteId         = v.ClienteId,
        ClienteNombre     = v.Cliente == null
            ? "Consumidor Final"
            : $"{v.Cliente.Person?.FirstName} {v.Cliente.Person?.LastName}".Trim(),
        RecetaId          = v.RecetaId,
        Estado            = v.Estado.ToString(),
        Tipo              = v.Tipo.ToString(),
        CondicionVenta    = v.CondicionVenta.ToString(),
        FechaVenta        = v.FechaVenta.ToString("yyyy-MM-dd"),
        ValidezDias       = v.ValidezDias,
        FechaConfirmacion = v.FechaConfirmacion?.ToString("yyyy-MM-dd"),
        FechaComprobante  = v.FechaComprobante?.ToString("yyyy-MM-dd"),
        MontoExento       = v.MontoExento,
        MontoGravado5     = v.MontoGravado5,
        MontoGravado10    = v.MontoGravado10,
        Total             = v.Total,
        MontoSeña         = v.MontoSeña,
        TotalCobrado      = v.TotalCobrado,
        SaldoPendiente    = v.SaldoPendiente,
        CantidadCuotas          = v.CantidadCuotas,
        FrecuenciaCuotasDias    = v.FrecuenciaCuotasDias,
        MontoCuota              = v.MontoCuota,
        CuotasPagadas           = v.CuotasPagadas,
        ProximaCuotaVencimiento = v.ProximaCuotaVencimiento?.ToString("yyyy-MM-dd"),
        CuotaVencida            = v.CuotaVencida,
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
                Referencia = l.Referencia,
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
            .Include(v => v.Sucursal)
            .Include(v => v.Cliente).ThenInclude(c => c!.Person)
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
        int? clienteId, int page, int pageSize)
    {
        var query = BaseQuery();

        if (current.SucursalId is int b)
            query = query.Where(v => v.SucursalId == b);

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoVenta>(estado, out var e))
            query = query.Where(v => v.Estado == e);

        if (!string.IsNullOrWhiteSpace(tipo) && Enum.TryParse<TipoVenta>(tipo, out var t))
            query = query.Where(v => v.Tipo == t);

        if (!string.IsNullOrWhiteSpace(fechaDesde) && DateOnly.TryParse(fechaDesde, out var desde))
            query = query.Where(v => v.FechaVenta >= desde);

        if (!string.IsNullOrWhiteSpace(fechaHasta) && DateOnly.TryParse(fechaHasta, out var hasta))
            query = query.Where(v => v.FechaVenta <= hasta);

        if (clienteId.HasValue)
            query = query.Where(v => v.ClienteId == clienteId.Value);

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

        // Cliente opcional (sin cliente = consumidor final). Si viene, debe existir y estar activo.
        if (request.ClienteId.HasValue)
        {
            var cliente = await db.Clientes.FirstOrDefaultAsync(x => x.Id == request.ClienteId.Value);
            if (cliente == null)
                return Result<VentaDto>.Failure("Cliente no encontrado", ErrorType.Validation);
            if (!cliente.IsActive)
                return Result<VentaDto>.Failure("El cliente está inactivo", ErrorType.Validation);
        }

        var condicion = Enum.TryParse<CondicionVenta>(request.CondicionVenta, out var c) ? c : CondicionVenta.Contado;
        var tipo      = Enum.TryParse<TipoVenta>(request.Tipo ?? "Directa", out var tv) ? tv : TipoVenta.Directa;

        if (tipo == TipoVenta.TrabajoAPedido && request.RecetaId is null)
            return Result<VentaDto>.Failure("La receta es obligatoria para una venta a pedido.", ErrorType.Validation);

        var venta = new Venta
        {
            NumeroComprobante = "",
            SucursalId        = await SucursalResolver.WriteBranchAsync(db, current),
            ClienteId         = request.ClienteId,
            VendedorId        = current.UserId,
            RecetaId          = request.RecetaId,
            CondicionVenta    = condicion,
            Tipo              = tipo,
            FechaVenta        = DateOnly.Parse(request.FechaVenta),
            ValidezDias       = request.ValidezDias < 1 ? 15 : request.ValidezDias,
            // El plan de cuotas solo tiene sentido en ventas a Crédito; en Contado se ignora
            // aunque venga en el request, para no dejar datos inconsistentes.
            CantidadCuotas        = condicion == CondicionVenta.Credito ? request.CantidadCuotas : null,
            FrecuenciaCuotasDias  = condicion == CondicionVenta.Credito ? request.FrecuenciaCuotasDias : null,
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

        // Trabajo a pedido (óptica): se crea junto al presupuesto en estado Borrador.
        // No entra a la cola del laboratorio hasta confirmar la venta.
        if (tipo == TipoVenta.TrabajoAPedido && request.TrabajoPedido is not null)
        {
            var tpReq = request.TrabajoPedido;
            var tratamientosTp = tpReq.TratamientoIds.Any()
                ? await db.Tratamientos.Where(t => tpReq.TratamientoIds.Contains(t.Id)).ToListAsync()
                : [];

            var tp = new TrabajoPedido
            {
                RecetaId               = request.RecetaId,
                TipoLenteId            = tpReq.TipoLenteId,
                ArmazonProductoId      = tpReq.ArmazonDelCliente ? null : tpReq.ArmazonProductoId,
                ArmazonDelCliente      = tpReq.ArmazonDelCliente,
                LaboratorioProveedorId = tpReq.LaboratorioProveedorId,
                Observacion            = tpReq.Observacion?.Trim(),
                Estado                 = EstadoTrabajoPedido.Borrador,
                CreatedAt              = DateTime.UtcNow,
                UpdatedAt              = DateTime.UtcNow,
            };
            foreach (var t in tratamientosTp) tp.Tratamientos.Add(t);
            venta.TrabajoPedido = tp;
        }

        db.Ventas.Add(venta);
        await db.SaveChangesAsync();

        venta.NumeroComprobante = $"REC-{venta.Id:D7}";
        await db.SaveChangesAsync();

        return await GetVentaByIdAsync(venta.Id);
    }

    public async Task<Result<VentaDto>> ActualizarVentaAsync(int id, ActualizarVentaRequest request)
    {
        if (!request.Lineas.Any())
            return Result<VentaDto>.Failure("La venta debe tener al menos una línea", ErrorType.Validation);

        var venta = await db.Ventas
            .Include(v => v.Lineas)
            .Include(v => v.TrabajoPedido)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);
        if (venta.Estado != EstadoVenta.Borrador)
            return Result<VentaDto>.Failure("Solo se pueden editar presupuestos en estado Borrador", ErrorType.Conflict);

        var condicion = Enum.TryParse<CondicionVenta>(request.CondicionVenta, out var c) ? c : venta.CondicionVenta;

        venta.CondicionVenta = condicion;
        venta.FechaVenta     = DateOnly.Parse(request.FechaVenta);
        venta.CantidadCuotas       = condicion == CondicionVenta.Credito ? request.CantidadCuotas : null;
        venta.FrecuenciaCuotasDias = condicion == CondicionVenta.Credito ? request.FrecuenciaCuotasDias : null;
        venta.Observaciones  = request.Observaciones;
        venta.UpdatedAt      = DateTime.UtcNow;

        // Asignación de laboratorio para el trabajo a pedido (necesario para poder confirmar).
        if (request.LaboratorioProveedorId.HasValue && venta.TrabajoPedido is not null)
        {
            var lab = await db.Proveedores.FirstOrDefaultAsync(p => p.Id == request.LaboratorioProveedorId.Value);
            if (lab == null)
                return Result<VentaDto>.Failure("Laboratorio no encontrado", ErrorType.Validation);
            if (!lab.EsLaboratorio)
                return Result<VentaDto>.Failure("El proveedor seleccionado no es un laboratorio", ErrorType.Validation);

            venta.TrabajoPedido.LaboratorioProveedorId = lab.Id;
            venta.TrabajoPedido.UpdatedAt              = DateTime.UtcNow;
        }

        // Reemplazo total de líneas: se eliminan las actuales y se recrean con el payload.
        db.VentaLineas.RemoveRange(venta.Lineas);
        venta.Lineas.Clear();

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

        await db.SaveChangesAsync();
        return await GetVentaByIdAsync(venta.Id);
    }

    public async Task<Result<VentaDto>> ConfirmarVentaAsync(int id, int userId)
    {
        var venta = await db.Ventas
            .Include(v => v.TrabajoPedido)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (venta == null) return Result<VentaDto>.Failure("Venta no encontrada", ErrorType.NotFound);
        if (!venta.PuedeConfirmarse())
            return Result<VentaDto>.Failure("Solo se pueden confirmar ventas en estado Borrador", ErrorType.Conflict);

        // Defensa adicional: CrearVentaAsync ya exige la receta para todo trabajo a pedido
        // (venta o presupuesto), así que esto nunca debería dispararse en la práctica.
        if (venta.Tipo == TipoVenta.TrabajoAPedido && venta.RecetaId is null)
            return Result<VentaDto>.Failure("La receta es obligatoria para confirmar una venta a pedido.", ErrorType.Conflict);

        // Diseño del lente obligatorio: define el cristal a fabricar en el laboratorio.
        if (venta.Tipo == TipoVenta.TrabajoAPedido && venta.TrabajoPedido?.TipoLenteId is null)
            return Result<VentaDto>.Failure("Seleccioná el diseño del lente antes de confirmar la venta a pedido.", ErrorType.Conflict);

        // El trabajo a pedido entra a la cola del laboratorio recién ahora.
        if (venta.TrabajoPedido is not null && venta.TrabajoPedido.Estado == EstadoTrabajoPedido.Borrador)
        {
            if (venta.Tipo == TipoVenta.TrabajoAPedido && venta.TrabajoPedido.LaboratorioProveedorId is null)
                return Result<VentaDto>.Failure("Asigná el laboratorio antes de confirmar la venta a pedido.", ErrorType.Conflict);
            venta.TrabajoPedido.Estado    = EstadoTrabajoPedido.PendienteAprobacion;
            venta.TrabajoPedido.UpdatedAt = DateTime.UtcNow;
        }

        // El cobro NO depende del laboratorio: toda venta confirmada queda lista para cobrar.
        // El pedido al laboratorio (si corresponde) sigue su circuito en paralelo y se envía
        // recién después del cobro.
        venta.Estado            = EstadoVenta.ListaParaCobrar;
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
        var venta = await db.Ventas
            .Include(v => v.Cobros).ThenInclude(c => c.Lineas)
            .Include(v => v.Devoluciones)
            .FirstOrDefaultAsync(v => v.Id == id);
        if (venta == null) return Result<bool>.Failure("Venta no encontrada.", ErrorType.NotFound);
        if (venta.Estado != EstadoVenta.Borrador && venta.Estado != EstadoVenta.ListaParaCobrar)
            return Result<bool>.Failure("Solo se pueden eliminar ventas en estado Borrador o Lista para Cobrar.", ErrorType.Conflict);
        if (venta.Devoluciones.Any())
            return Result<bool>.Failure("La venta tiene devoluciones registradas. No se puede eliminar.", ErrorType.Conflict);

        if (venta.Cobros.Any())
        {
            db.CobroLineas.RemoveRange(venta.Cobros.SelectMany(c => c.Lineas));
            db.Cobros.RemoveRange(venta.Cobros);
        }

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

        var lineas = new List<(MetodoPago metodo, decimal monto, string? referencia)>();
        foreach (var l in request.Lineas)
        {
            if (!Enum.TryParse<MetodoPago>(l.MetodoPago, out var metodo))
                return Result<VentaDto>.Failure($"Método de pago inválido: {l.MetodoPago}", ErrorType.Validation);
            if (l.Monto <= 0)
                return Result<VentaDto>.Failure("El monto de cada línea debe ser mayor a cero", ErrorType.Validation);
            var referencia = string.IsNullOrWhiteSpace(l.Referencia) ? null : l.Referencia.Trim();
            lineas.Add((metodo, l.Monto, referencia));
        }

        var montoTotal = lineas.Sum(l => l.monto);
        if (montoTotal > venta.SaldoPendiente)
            return Result<VentaDto>.Failure(
                $"El monto del cobro ({montoTotal:N0}) supera el saldo pendiente ({venta.SaldoPendiente:N0})",
                ErrorType.Validation);

        // Todo cobro debe quedar asociado a una sesión de caja abierta.
        var sesionCaja = await db.SesionesCaja.FirstOrDefaultAsync(s => s.Estado == EstadoSesionCaja.Abierta && s.SucursalId == venta.SucursalId);
        if (sesionCaja is null)
            return Result<VentaDto>.Failure(
                "No hay una caja abierta. Abrí la caja antes de registrar cobros.", ErrorType.Conflict);

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

        foreach (var (metodo, monto, referencia) in lineas)
        {
            cobro.Lineas.Add(new CobroLinea { MetodoPago = metodo, Monto = monto, Referencia = referencia });

            db.MovimientosCaja.Add(new MovimientoCaja
            {
                Tipo            = TipoMovimientoCaja.Ingreso,
                Monto           = monto,
                Concepto        = $"Cobro {tipoCobro} — venta {venta.NumeroComprobante}",
                MetodoPago      = metodo,
                SucursalId      = venta.SucursalId,
                VentaId         = venta.Id,
                SesionCajaId    = sesionCaja?.Id,
                RegistradoPorId = userId,
                Fecha           = fecha,
                Referencia      = referencia,
                CreatedAt       = DateTime.UtcNow,
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
        // Documento fiscal excluyente: no se puede emitir recibo si ya hay factura.
        if (venta.Factura != null)
            return Result<VentaDto>.Failure("La venta ya tiene factura emitida", ErrorType.Conflict);

        var now = DateTime.UtcNow;

        SesionCaja? sesionCaja = null;
        if (GeneraMovimientoEfectivoEmision(venta))
        {
            sesionCaja = await db.SesionesCaja.FirstOrDefaultAsync(s => s.Estado == EstadoSesionCaja.Abierta && s.SucursalId == venta.SucursalId);
            if (sesionCaja is null)
                return Result<VentaDto>.Failure(
                    "No hay una caja abierta. Abrí la caja antes de registrar la venta de contado en efectivo.", ErrorType.Conflict);
        }

        AplicarEgresosDeEmision(venta, now, sesionCaja, venta.SucursalId);

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
        if (!venta.PuedeEmitirComprobante())
            return Result<VentaDto>.Failure(
                "Solo se puede emitir factura cuando la venta está lista para cobrar", ErrorType.Conflict);
        if (venta.Factura != null)
            return Result<VentaDto>.Failure("La venta ya tiene factura emitida", ErrorType.Conflict);
        if (venta.Comprobante != null)
            return Result<VentaDto>.Failure("La venta ya tiene comprobante emitido", ErrorType.Conflict);

        var timbrado = await db.Timbrados.FindAsync(request.TimbradoId);
        if (timbrado == null)
            return Result<VentaDto>.Failure("Timbrado no encontrado.", ErrorType.NotFound);
        if (!timbrado.IsActive)
            return Result<VentaDto>.Failure("El timbrado está inactivo.", ErrorType.Conflict);
        if (timbrado.SucursalId != venta.SucursalId)
            return Result<VentaDto>.Failure("El timbrado no pertenece a la sucursal de la venta.", ErrorType.Conflict);

        var fechaEmision = DateOnly.Parse(request.FechaEmision);
        if (fechaEmision < timbrado.FechaInicioVigencia || fechaEmision > timbrado.FechaFinVigencia)
            return Result<VentaDto>.Failure(
                $"La fecha de emisión debe estar dentro del período de vigencia del timbrado ({timbrado.FechaInicioVigencia:yyyy-MM-dd} al {timbrado.FechaFinVigencia:yyyy-MM-dd}).",
                ErrorType.Validation);

        var numero = timbrado.UltimoNumero + 1;
        if (timbrado.NumeroHasta.HasValue && numero > timbrado.NumeroHasta.Value)
            return Result<VentaDto>.Failure(
                $"Se alcanzó el número máximo del timbrado ({timbrado.NumeroHasta}).",
                ErrorType.Conflict);

        var numeroFactura = $"{timbrado.Establecimiento}-{timbrado.PuntoExpedicion}-{numero:D7}";
        var now = DateTime.UtcNow;

        db.FacturasVenta.Add(new FacturaVenta
        {
            VentaId         = request.VentaId,
            NumeroFactura   = numeroFactura,
            Timbrado        = timbrado.NumeroTimbrado,
            Establecimiento = timbrado.Establecimiento,
            MontoExento     = venta.MontoExento,
            MontoGravado5   = venta.MontoGravado5,
            MontoGravado10  = venta.MontoGravado10,
            FechaEmision    = fechaEmision,
            Observaciones   = request.Observaciones,
            TimbradoId      = timbrado.Id,
            CreatedAt       = now,
        });

        SesionCaja? sesionCaja = null;
        if (GeneraMovimientoEfectivoEmision(venta))
        {
            sesionCaja = await db.SesionesCaja.FirstOrDefaultAsync(s => s.Estado == EstadoSesionCaja.Abierta && s.SucursalId == venta.SucursalId);
            if (sesionCaja is null)
                return Result<VentaDto>.Failure(
                    "No hay una caja abierta. Abrí la caja antes de registrar la venta de contado en efectivo.", ErrorType.Conflict);
        }

        timbrado.UltimoNumero = numero;

        AplicarEgresosDeEmision(venta, now, sesionCaja, venta.SucursalId);

        venta.Estado           = EstadoVenta.ComprobanteEmitido;
        venta.FechaComprobante = DateOnly.FromDateTime(now);
        venta.UpdatedAt        = now;

        await db.SaveChangesAsync();
        return await GetVentaByIdAsync(request.VentaId);
    }

    /// <summary>
    /// Efectos comunes a la emisión de un documento fiscal (recibo o factura):
    /// EGRESO de stock por cada línea de producto e INGRESO de caja para ventas de contado
    /// que aún no registraron cobros (evita duplicar la caja si ya hubo cobros/seña/recibo).
    /// </summary>
    private void AplicarEgresosDeEmision(Venta venta, DateTime now, SesionCaja? sesion, int sucursalId)
    {
        // Egreso de stock por cada línea de producto
        foreach (var linea in venta.Lineas.Where(l => l.Tipo == TipoLineaVenta.Producto && l.ProductoId.HasValue))
        {
            db.MovimientosStock.Add(new MovimientoStock
            {
                ProductoId      = linea.ProductoId!.Value,
                SucursalId      = sucursalId,
                Tipo            = "Salida",
                Cantidad        = linea.Cantidad,
                Motivo          = $"Comprobante venta {venta.NumeroComprobante}",
                FechaMovimiento = now,
                Estado          = "Aprobado",
                CreatedAt       = now,
            });
        }

        // Movimiento de caja para CONTADO solo si no se registraron cobros con método
        // (si ya hay cobros, la caja se registró en cada cobro; no duplicar).
        if (GeneraMovimientoEfectivoEmision(venta))
        {
            db.MovimientosCaja.Add(new MovimientoCaja
            {
                Tipo         = TipoMovimientoCaja.Ingreso,
                Monto        = venta.Total,
                Concepto     = $"Comprobante venta {venta.NumeroComprobante}",
                MetodoPago   = MetodoPago.Efectivo,
                SucursalId   = venta.SucursalId,
                VentaId      = venta.Id,
                SesionCajaId = sesion?.Id,
                Fecha        = DateOnly.FromDateTime(now),
                CreatedAt    = now,
            });
        }
    }

    /// <summary>
    /// La emisión de un documento de CONTADO sin cobros previos genera el ingreso de caja en
    /// efectivo por el total de la venta. (Si ya hubo cobros, la caja se registró en cada cobro.)
    /// </summary>
    private static bool GeneraMovimientoEfectivoEmision(Venta venta) =>
        venta.CondicionVenta == CondicionVenta.Contado && !venta.Cobros.Any(c => !c.Anulado);

    private static TrabajoPedidoDto MapTrabajoPedidoDto(TrabajoPedido tp) => new()
    {
        Id                     = tp.Id,
        RecetaId               = tp.RecetaId,
        TipoLenteId            = tp.TipoLenteId,
        TipoLenteNombre        = tp.TipoLente?.Nombre,
        Tratamientos           = tp.Tratamientos.Select(t => new TrabajoPedidoTratamientoDto { Id = t.Id, Nombre = t.Nombre }).ToList(),
        ArmazonProductoId      = tp.ArmazonProductoId,
        ArmazonProductoNombre  = tp.ArmazonProducto?.Nombre,
        ArmazonDelCliente      = tp.ArmazonDelCliente,
        LaboratorioProveedorId = tp.LaboratorioProveedorId,
        LaboratorioNombre      = tp.LaboratorioProveedor?.Nombre,
        Estado                 = tp.Estado.ToString(),
        FechaEnvio             = tp.FechaEnvio?.ToString("yyyy-MM-dd"),
        FechaRecepcion         = tp.FechaRecepcion?.ToString("yyyy-MM-dd"),
        Observacion            = tp.Observacion,
        CreatedAt              = tp.CreatedAt,
    };

    private static DevolucionDto MapDevolucion(Devolucion d) => new()
    {
        Id                    = d.Id,
        VentaId               = d.VentaId,
        NumeroComprobante     = d.Venta?.NumeroComprobante ?? "",
        ClienteNombre         = d.Venta?.Cliente == null
            ? "Consumidor Final"
            : $"{d.Venta.Cliente.Person?.FirstName} {d.Venta.Cliente.Person?.LastName}".Trim(),
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
                v.Estado != EstadoVenta.Cancelada &&
                v.Estado != EstadoVenta.Borrador &&
                // Contado pendiente de cobro/comprobante (estado ListaParaCobrar)
                // o cualquier venta a crédito activa (el saldo se filtra abajo).
                (v.CondicionVenta == CondicionVenta.Credito ||
                 v.Estado == EstadoVenta.ListaParaCobrar))
            .OrderBy(v => v.FechaVenta)
            .ToListAsync();

        // Contado: aparece mientras esté ListaParaCobrar (aún sin comprobante).
        // Crédito: solo si conserva saldo pendiente real (requiere cobros cargados).
        var pendientes = ventas
            .Where(v => v.CondicionVenta == CondicionVenta.Credito
                ? v.SaldoPendiente > 0
                : v.Estado == EstadoVenta.ListaParaCobrar)
            .Select(Map)
            .ToList();
        return Result<List<VentaDto>>.Success(pendientes);
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

    public async Task<Result<List<DevolucionDto>>> GetDevolucionesPendientesAsync()
    {
        var devoluciones = await db.Devoluciones
            .Include(d => d.Venta).ThenInclude(v => v.Cliente).ThenInclude(c => c!.Person)
            .Include(d => d.Lineas).ThenInclude(l => l.ProductoDevuelto)
            .Include(d => d.Lineas).ThenInclude(l => l.ProductoNuevo)
            .Include(d => d.SolicitadoPor).ThenInclude(u => u.Person)
            .Where(d => d.Estado == EstadoDevolucion.Pendiente)
            .OrderBy(d => d.CreatedAt)
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
        var sucursalId = devolucion.Venta.SucursalId;

        foreach (var linea in devolucion.Lineas)
        {
            // INGRESO stock del producto devuelto
            db.MovimientosStock.Add(new MovimientoStock
            {
                ProductoId      = linea.ProductoDevueltoId,
                SucursalId      = sucursalId,
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
                    SucursalId      = sucursalId,
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
                var sesionDevolucion = await db.SesionesCaja
                    .FirstOrDefaultAsync(s => s.Estado == EstadoSesionCaja.Abierta && s.SucursalId == sucursalId);

                db.MovimientosCaja.Add(new MovimientoCaja
                {
                    Tipo         = TipoMovimientoCaja.Egreso,
                    Monto        = totalDevuelto,
                    Concepto     = $"Devolución #{devolucion.Id} — venta {devolucion.Venta.NumeroComprobante}",
                    MetodoPago   = MetodoPago.Efectivo,
                    SucursalId   = sucursalId,
                    VentaId      = devolucion.VentaId,
                    SesionCajaId = sesionDevolucion?.Id,
                    Fecha        = fecha,
                    CreatedAt    = now,
                });
            }
        }

        await db.SaveChangesAsync();
        return Result<DevolucionDto>.Success(MapDevolucion(devolucion));
    }
}
