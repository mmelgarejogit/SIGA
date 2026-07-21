using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class LaboratorioService(AppDbContext db, ICurrentUserContext current, INotificacionInternaService notificacion) : ILaboratorioService
{
    public async Task<Result<List<TrabajoPedidoListDto>>> GetPedidosAsync(string? estado)
    {
        var query = TpBaseQuery()
            .Where(tp => tp.Estado != EstadoTrabajoPedido.Borrador);

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

        tp.AprobadoPorId         = userId;
        tp.ObservacionAprobacion = request.Observacion?.Trim();
        tp.Estado                = request.Accion == "Aprobar" ? EstadoTrabajoPedido.PendienteEnvio : EstadoTrabajoPedido.Rechazado;
        tp.UpdatedAt             = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Result<TrabajoPedidoListDto>.Success(MapTrabajoPedidoList(tp));
    }

    public async Task<Result<TrabajoPedidoListDto>> RegistrarEnvioAsync(int id, RegistrarEnvioRequest request)
    {
        var tp = await TpBaseQuery().FirstOrDefaultAsync(t => t.Id == id);
        if (tp is null) return Result<TrabajoPedidoListDto>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (tp.Estado != EstadoTrabajoPedido.PendienteEnvio)
            return Result<TrabajoPedidoListDto>.Failure("El pedido no está aprobado para envío.", ErrorType.Conflict);

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        DateOnly? fechaEstimada = null;
        if (!string.IsNullOrWhiteSpace(request.FechaEstimadaEntrega))
        {
            if (!DateOnly.TryParse(request.FechaEstimadaEntrega, out var fe))
                return Result<TrabajoPedidoListDto>.Failure("Fecha estimada de entrega inválida.", ErrorType.Validation);
            if (fe < hoy)
                return Result<TrabajoPedidoListDto>.Failure("La fecha estimada de entrega no puede ser anterior a hoy.", ErrorType.Validation);
            fechaEstimada = fe;
        }

        MedioEnvioLaboratorio? medio = null;
        if (!string.IsNullOrWhiteSpace(request.MedioEnvio))
        {
            if (!Enum.TryParse<MedioEnvioLaboratorio>(request.MedioEnvio, out var m))
                return Result<TrabajoPedidoListDto>.Failure("Medio de envío inválido.", ErrorType.Validation);
            medio = m;
        }

        tp.Estado               = EstadoTrabajoPedido.Enviado;
        tp.FechaEnvio           = hoy;
        tp.FechaEstimadaEntrega = fechaEstimada;
        tp.MedioEnvio           = medio;
        tp.UpdatedAt            = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Result<TrabajoPedidoListDto>.Success(MapTrabajoPedidoList(tp));
    }

    public async Task<Result<TrabajoPedidoListDto>> RegistrarRecepcionAsync(int id)
    {
        var tp = await TpBaseQuery().FirstOrDefaultAsync(t => t.Id == id);
        if (tp is null) return Result<TrabajoPedidoListDto>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (tp.Estado != EstadoTrabajoPedido.Enviado)
            return Result<TrabajoPedidoListDto>.Failure("El pedido debe estar en estado Enviado.", ErrorType.Conflict);

        var now = DateTime.UtcNow;
        tp.Estado         = EstadoTrabajoPedido.Recibido;
        tp.FechaRecepcion = DateOnly.FromDateTime(now);
        tp.UpdatedAt      = now;

        // La recepción del laboratorio NO altera el estado de la venta: el cobro es
        // independiente del circuito de laboratorio (la venta ya se cobró antes de enviar).
        await db.SaveChangesAsync();

        await notificacion.CrearAsync(
            tipo: TipoNotificacion.PedidoLabRecibido,
            mensaje: $"El pedido de laboratorio #{tp.Id} llegó y está listo para retirar.",
            entidadOrigenTipo: "TrabajoPedido",
            entidadOrigenId: tp.Id,
            destinatarioSucursalId: tp.Venta.SucursalId);

        return Result<TrabajoPedidoListDto>.Success(MapTrabajoPedidoList(tp));
    }

    public async Task<Result<TrabajoPedidoListDto>> RegistrarEntregaAsync(int id, RegistrarEntregaRequest request, int userId)
    {
        var tp = await TpBaseQuery().FirstOrDefaultAsync(t => t.Id == id);
        if (tp is null) return Result<TrabajoPedidoListDto>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (tp.Estado != EstadoTrabajoPedido.Recibido)
            return Result<TrabajoPedidoListDto>.Failure(
                "Solo se pueden entregar pedidos recibidos del laboratorio (listos para retirar).", ErrorType.Conflict);

        // No se entregan los lentes sin haber emitido el comprobante/factura de la venta.
        if (tp.Venta.Estado != EstadoVenta.ComprobanteEmitido)
            return Result<TrabajoPedidoListDto>.Failure(
                "Antes de entregar hay que emitir el comprobante de la venta (cobrar el saldo y emitir factura/recibo).",
                ErrorType.Conflict);

        var now = DateTime.UtcNow;
        tp.Estado         = EstadoTrabajoPedido.Entregado;
        tp.FechaEntrega   = DateOnly.FromDateTime(now);
        tp.EntregadoPorId = userId;
        tp.RetiradoPor    = string.IsNullOrWhiteSpace(request.RetiradoPor) ? null : request.RetiradoPor.Trim();
        tp.UpdatedAt      = now;

        await db.SaveChangesAsync();

        var saved = await TpBaseQuery().FirstAsync(t => t.Id == id);
        return Result<TrabajoPedidoListDto>.Success(MapTrabajoPedidoList(saved));
    }

    public async Task<Result<TrabajoPedidoListDto>> RegistrarRetrabajoAsync(int id, RegistrarRetrabajoRequest request, int userId)
    {
        if (!Enum.TryParse<MotivoRetrabajo>(request.Motivo, out var motivo))
            return Result<TrabajoPedidoListDto>.Failure("Motivo de re-trabajo inválido.", ErrorType.Validation);
        if (!Enum.TryParse<ResponsableRetrabajo>(request.Responsable, out var responsable))
            return Result<TrabajoPedidoListDto>.Failure("Responsable del re-trabajo inválido.", ErrorType.Validation);

        var tp = await TpBaseQuery().FirstOrDefaultAsync(t => t.Id == id);
        if (tp is null) return Result<TrabajoPedidoListDto>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        // Solo se rehace algo que ya volvió del laboratorio (recibido o incluso entregado y devuelto).
        if (tp.Estado is not (EstadoTrabajoPedido.Recibido or EstadoTrabajoPedido.Entregado))
            return Result<TrabajoPedidoListDto>.Failure(
                "Solo se puede rehacer un trabajo recibido o entregado (que volvió del laboratorio).", ErrorType.Conflict);

        var now = DateTime.UtcNow;
        tp.Retrabajos.Add(new RetrabajoTrabajoPedido
        {
            Fecha           = DateOnly.FromDateTime(now),
            Motivo          = motivo,
            Responsable     = responsable,
            Observacion     = string.IsNullOrWhiteSpace(request.Observacion) ? null : request.Observacion.Trim(),
            RegistradoPorId = userId,
            CreatedAt       = now,
        });

        // El trabajo vuelve a la cola de envíos para rehacerse; se limpia el ciclo anterior
        // (envío/recepción/entrega) — el historial queda en el log de re-trabajos.
        tp.Estado               = EstadoTrabajoPedido.PendienteEnvio;
        tp.FechaEnvio           = null;
        tp.FechaRecepcion       = null;
        tp.FechaEstimadaEntrega = null;
        tp.MedioEnvio           = null;
        tp.FechaEntrega         = null;
        tp.EntregadoPorId       = null;
        tp.RetiradoPor          = null;
        tp.UpdatedAt            = now;

        await db.SaveChangesAsync();

        var saved = await TpBaseQuery().FirstAsync(t => t.Id == id);
        return Result<TrabajoPedidoListDto>.Success(MapTrabajoPedidoList(saved));
    }

    public async Task<Result<TrabajoPedidoListDto>> EmitirFacturaAsync(
        int id, EmitirFacturaLaboratorioRequest request, int userId)
    {
        var tp = await TpBaseQuery().FirstOrDefaultAsync(t => t.Id == id);
        if (tp is null) return Result<TrabajoPedidoListDto>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        // El laboratorio puede facturarnos aun después de que el cliente ya retiró.
        if (tp.Estado is not (EstadoTrabajoPedido.Recibido or EstadoTrabajoPedido.Entregado))
            return Result<TrabajoPedidoListDto>.Failure("Solo se puede emitir factura de pedidos recibidos o entregados.", ErrorType.Conflict);

        if (tp.Factura != null)
            return Result<TrabajoPedidoListDto>.Failure("Este pedido ya tiene una factura registrada.", ErrorType.Conflict);

        var fl = new FacturaLaboratorio
        {
            TrabajoPedidoId = id,
            NumeroFactura   = request.NumeroFactura.Trim(),
            Timbrado        = request.Timbrado?.Trim(),
            FechaEmision    = DateOnly.Parse(request.FechaEmision),
            Monto           = request.Monto,
            Observaciones   = request.Observaciones?.Trim(),
            EmitidoPorId    = userId,
            CreatedAt       = DateTime.UtcNow,
        };
        db.FacturasLaboratorio.Add(fl);

        var labNombre     = tp.LaboratorioProveedor?.Nombre ?? "laboratorio";
        var clienteNombre = tp.Venta?.Cliente == null
            ? "Consumidor Final"
            : $"{tp.Venta.Cliente.Person?.FirstName} {tp.Venta.Cliente.Person?.LastName}".Trim();

        db.EgresosFacturaLaboratorio.Add(new EgresoFacturaLaboratorio
        {
            FacturaLaboratorio = fl,
            Monto              = request.Monto,
            Concepto           = $"Factura lab. {labNombre} — {tp.Venta?.NumeroComprobante ?? $"Pedido #{id}"} ({clienteNombre})",
            Observaciones      = request.Observaciones?.Trim(),
            FechaEmision       = DateOnly.Parse(request.FechaEmision),
            Estado             = EstadoEgreso.Pendiente,
        });

        await db.SaveChangesAsync();

        var saved = await TpBaseQuery().FirstAsync(t => t.Id == id);
        return Result<TrabajoPedidoListDto>.Success(MapTrabajoPedidoList(saved));
    }

    // ── Helpers ──────────────────────────────────────────────────────────────────

    private IQueryable<TrabajoPedido> TpBaseQuery()
    {
        var query = db.TrabajosPedido
            .Include(tp => tp.Venta).ThenInclude(v => v!.Cliente).ThenInclude(c => c!.Person)
            .Include(tp => tp.TipoLente)
            .Include(tp => tp.ArmazonProducto)
            .Include(tp => tp.Receta)
            .Include(tp => tp.Tratamientos)
            .Include(tp => tp.LaboratorioProveedor)
            .Include(tp => tp.AprobadoPor).ThenInclude(u => u!.Person)
            .Include(tp => tp.EntregadoPor).ThenInclude(u => u!.Person)
            .Include(tp => tp.Retrabajos).ThenInclude(r => r.RegistradoPor).ThenInclude(u => u!.Person)
            .Include(tp => tp.Factura).ThenInclude(f => f!.EmitidoPor).ThenInclude(u => u.Person)
            .AsQueryable();

        // Los trabajos de laboratorio se scopean por la sucursal de su venta.
        if (current.SucursalId is int b)
            query = query.Where(tp => tp.Venta.SucursalId == b);

        return query;
    }

    private static TrabajoPedidoListDto MapTrabajoPedidoList(TrabajoPedido tp) => new()
    {
        Id                    = tp.Id,
        VentaId               = tp.VentaId,
        NumeroComprobante     = tp.Venta?.NumeroComprobante ?? "",
        ClienteNombre         = tp.Venta?.Cliente == null
            ? "Consumidor Final"
            : $"{tp.Venta.Cliente.Person?.FirstName} {tp.Venta.Cliente.Person?.LastName}".Trim(),
        TipoLenteNombre       = tp.TipoLente?.Nombre ?? "",
        Tratamientos          = tp.Tratamientos.Select(t => t.Nombre).ToList(),
        LaboratorioNombre     = tp.LaboratorioProveedor?.Nombre ?? "",
        Estado                = tp.Estado.ToString(),
        ObservacionAprobacion = tp.ObservacionAprobacion,
        AprobadoPorNombre     = tp.AprobadoPor == null ? null : $"{tp.AprobadoPor.Person?.FirstName} {tp.AprobadoPor.Person?.LastName}".Trim(),
        FechaEnvio            = tp.FechaEnvio?.ToString("yyyy-MM-dd"),
        FechaEstimadaEntrega  = tp.FechaEstimadaEntrega?.ToString("yyyy-MM-dd"),
        MedioEnvio            = tp.MedioEnvio?.ToString(),
        FechaRecepcion        = tp.FechaRecepcion?.ToString("yyyy-MM-dd"),
        FechaEntrega          = tp.FechaEntrega?.ToString("yyyy-MM-dd"),
        RetiradoPor           = tp.RetiradoPor,
        EntregadoPorNombre    = tp.EntregadoPor == null ? null : $"{tp.EntregadoPor.Person?.FirstName} {tp.EntregadoPor.Person?.LastName}".Trim(),
        Retrabajos            = tp.Retrabajos
            .OrderByDescending(r => r.Fecha).ThenByDescending(r => r.Id)
            .Select(r => new RetrabajoDto
            {
                Fecha              = r.Fecha.ToString("yyyy-MM-dd"),
                Motivo             = r.Motivo.ToString(),
                Responsable        = r.Responsable.ToString(),
                Observacion        = r.Observacion,
                RegistradoPorNombre = r.RegistradoPor == null ? null : $"{r.RegistradoPor.Person?.FirstName} {r.RegistradoPor.Person?.LastName}".Trim(),
            }).ToList(),
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
        ArmazonNombre         = tp.ArmazonProducto?.Nombre,
        ArmazonDelCliente     = tp.ArmazonDelCliente,
        Receta                = tp.Receta == null ? null : new RecetaRefDto
        {
            FechaEmision          = tp.Receta.FechaEmision.ToString("yyyy-MM-dd"),
            OdEsferico            = tp.Receta.OdEsferico,
            OdCilindro            = tp.Receta.OdCilindro,
            OdEje                 = tp.Receta.OdEje,
            OdAdicion             = tp.Receta.OdAdicion,
            OiEsferico            = tp.Receta.OiEsferico,
            OiCilindro            = tp.Receta.OiCilindro,
            OiEje                 = tp.Receta.OiEje,
            OiAdicion             = tp.Receta.OiAdicion,
            DistanciaInterpupilar = tp.Receta.DistanciaInterpupilar,
            Observaciones         = tp.Receta.Observaciones,
        },
        CreatedAt = tp.CreatedAt,
    };
}
