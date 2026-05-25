using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Compras;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class ComprasService(AppDbContext db) : IComprasService
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Crear pedido (Borrador)
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<Result<PedidoResponse>> CrearPedidoAsync(CrearPedidoRequest request)
    {
        var proveedor = await db.Proveedores.FindAsync(request.ProveedorId);
        if (proveedor is null)
            return Result<PedidoResponse>.Failure("Proveedor no encontrado.", ErrorType.NotFound);

        var items = request.Items.ToList();
        if (items.Count == 0)
            return Result<PedidoResponse>.Failure("El pedido debe tener al menos un ítem.", ErrorType.Validation);

        foreach (var item in items)
        {
            if (item.Cantidad <= 0)
                return Result<PedidoResponse>.Failure("La cantidad de cada ítem debe ser mayor a cero.", ErrorType.Validation);

            if (item.PrecioUnitario < 0)
                return Result<PedidoResponse>.Failure("El precio unitario no puede ser negativo.", ErrorType.Validation);

            var existe = await db.Productos.AnyAsync(p => p.Id == item.ProductoId && p.IsActive);
            if (!existe)
                return Result<PedidoResponse>.Failure($"Producto {item.ProductoId} no encontrado.", ErrorType.NotFound);
        }

        var pedido = new PedidoProveedor
        {
            ProveedorId   = request.ProveedorId,
            Estado        = EstadoPedido.Borrador,
            Observaciones = request.Observaciones?.Trim(),
            Items         = items.Select(i => new PedidoProveedorItem
            {
                ProductoId     = i.ProductoId,
                Cantidad       = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
            }).ToList(),
        };

        db.PedidosProveedor.Add(pedido);
        await db.SaveChangesAsync();

        return Result<PedidoResponse>.Success(await LoadAndMapAsync(pedido.Id));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Confirmar pedido: Borrador → Confirmada
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<Result<PedidoResponse>> ConfirmarPedidoAsync(int id)
    {
        var pedido = await db.PedidosProveedor.FindAsync(id);
        if (pedido is null)
            return Result<PedidoResponse>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (pedido.Estado != EstadoPedido.Borrador)
            return Result<PedidoResponse>.Failure("Solo se pueden confirmar pedidos en estado Borrador.", ErrorType.Validation);

        pedido.Estado    = EstadoPedido.Confirmada;
        pedido.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Result<PedidoResponse>.Success(await LoadAndMapAsync(id));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Registrar factura del proveedor: Confirmada → Facturada
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<Result<PedidoResponse>> RegistrarFacturaAsync(int id, RegistrarFacturaPedidoRequest request)
    {
        var pedido = await db.PedidosProveedor.Include(p => p.Proveedor).FirstOrDefaultAsync(p => p.Id == id);
        if (pedido is null)
            return Result<PedidoResponse>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (pedido.Estado != EstadoPedido.Confirmada)
            return Result<PedidoResponse>.Failure("Solo se puede registrar factura en pedidos Confirmados.", ErrorType.Validation);

        var facturaExistente = await db.FacturasCompra.AnyAsync(f => f.PedidoProveedorId == id);
        if (facturaExistente)
            return Result<PedidoResponse>.Failure("Este pedido ya tiene una factura registrada.", ErrorType.Validation);

        // Validar NroFactura único por proveedor
        var nroFactura = request.NroFactura.Trim();
        var duplicado = await db.FacturasCompra.AnyAsync(f =>
            f.ProveedorId == pedido.ProveedorId &&
            f.NroFactura == nroFactura);
        if (duplicado)
            return Result<PedidoResponse>.Failure($"La factura {nroFactura} ya fue registrada para este proveedor.", ErrorType.Conflict);

        if (!DateOnly.TryParse(request.FechaEmision, out var fechaEmision))
            return Result<PedidoResponse>.Failure("Fecha de emisión inválida.", ErrorType.Validation);

        DateOnly? fechaVencimiento = null;
        if (!string.IsNullOrWhiteSpace(request.FechaVencimiento))
        {
            if (!DateOnly.TryParse(request.FechaVencimiento, out var fv))
                return Result<PedidoResponse>.Failure("Fecha de vencimiento inválida.", ErrorType.Validation);
            fechaVencimiento = fv;
        }

        if (!Enum.TryParse<CondicionVenta>(request.CondicionVenta, ignoreCase: true, out var condicion))
            return Result<PedidoResponse>.Failure("Condición de venta inválida. Valores: Contado, Credito.", ErrorType.Validation);

        var factura = new FacturaCompra
        {
            ProveedorId       = pedido.ProveedorId,
            PedidoProveedorId = id,
            NroFactura        = nroFactura,
            MontoExento       = request.MontoExento,
            MontoGravado5     = request.MontoGravado5,
            MontoGravado10    = request.MontoGravado10,
            CondicionVenta    = condicion,
            Concepto          = $"Factura de compra — OC #{id} — {pedido.Proveedor.Nombre}",
            Observaciones     = request.Observaciones?.Trim(),
            FechaEmision      = fechaEmision,
            FechaVencimiento  = fechaVencimiento,
            Estado            = EstadoEgreso.Pendiente,
        };
        factura.Monto = factura.MontoTotal;

        db.FacturasCompra.Add(factura);

        pedido.Estado    = EstadoPedido.Facturada;
        pedido.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Result<PedidoResponse>.Success(await LoadAndMapAsync(id));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Registrar recepción: Facturada → RecibidaParcial/RecibidaTotal
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<Result<PedidoResponse>> RegistrarRecepcionAsync(int id, RegistrarRecepcionRequest request)
    {
        var pedido = await db.PedidosProveedor
            .Include(p => p.Items).ThenInclude(i => i.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
            return Result<PedidoResponse>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (pedido.Estado != EstadoPedido.Facturada && pedido.Estado != EstadoPedido.RecibidaParcial)
            return Result<PedidoResponse>.Failure(
                "Solo se puede registrar recepción en pedidos Facturados o Recibidos Parcialmente.",
                ErrorType.Validation);

        var recepciones = request.Items.ToList();
        if (recepciones.Count == 0)
            return Result<PedidoResponse>.Failure("Debe indicar al menos un ítem a recibir.", ErrorType.Validation);

        var recepcion = new RecepcionMercaderia
        {
            PedidoProveedorId = id,
            Observaciones     = request.Observaciones?.Trim(),
        };

        foreach (var rec in recepciones)
        {
            var item = pedido.Items.FirstOrDefault(i => i.Id == rec.ItemId);
            if (item is null)
                return Result<PedidoResponse>.Failure($"Ítem {rec.ItemId} no pertenece a este pedido.", ErrorType.Validation);

            if (rec.CantidadRecibida <= 0)
                return Result<PedidoResponse>.Failure("La cantidad recibida debe ser mayor a cero.", ErrorType.Validation);

            var pendiente = item.Cantidad - item.CantidadRecibida;
            if (rec.CantidadRecibida > pendiente)
                return Result<PedidoResponse>.Failure(
                    $"La cantidad recibida para el ítem {rec.ItemId} supera la cantidad pendiente ({pendiente}).",
                    ErrorType.Validation);

            recepcion.Items.Add(new RecepcionMercaderiaItem
            {
                PedidoItemId = item.Id,
                Cantidad     = rec.CantidadRecibida,
            });

            item.CantidadRecibida += rec.CantidadRecibida;

            item.Producto.StockActual += rec.CantidadRecibida;
            item.Producto.UpdatedAt   =  DateTime.UtcNow;

            db.MovimientosStock.Add(new MovimientoStock
            {
                ProductoId = item.ProductoId,
                Tipo       = "Entrada",
                Cantidad   = rec.CantidadRecibida,
                Motivo     = $"Recepción — OC #{pedido.Id}",
            });
        }

        db.RecepcionesMercaderia.Add(recepcion);

        pedido.Estado    = pedido.Items.All(i => i.CantidadRecibida >= i.Cantidad)
            ? EstadoPedido.RecibidaTotal
            : EstadoPedido.RecibidaParcial;
        pedido.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Result<PedidoResponse>.Success(await LoadAndMapAsync(id));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Registrar devolución (no cambia estado)
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<Result<DevolucionResponse>> RegistrarDevolucionAsync(int id, RegistrarDevolucionRequest request)
    {
        var pedido = await db.PedidosProveedor
            .Include(p => p.Items).ThenInclude(i => i.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
            return Result<DevolucionResponse>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (pedido.Estado != EstadoPedido.RecibidaTotal && pedido.Estado != EstadoPedido.RecibidaParcial)
            return Result<DevolucionResponse>.Failure(
                "Solo se pueden registrar devoluciones en pedidos con mercadería recibida.",
                ErrorType.Validation);

        var item = pedido.Items.FirstOrDefault(i => i.Id == request.ItemId);
        if (item is null)
            return Result<DevolucionResponse>.Failure("Ítem no encontrado en este pedido.", ErrorType.NotFound);

        if (request.Cantidad <= 0)
            return Result<DevolucionResponse>.Failure("La cantidad a devolver debe ser mayor a cero.", ErrorType.Validation);

        if (request.Cantidad > item.CantidadRecibida)
            return Result<DevolucionResponse>.Failure(
                $"La cantidad a devolver ({request.Cantidad}) supera la cantidad recibida ({item.CantidadRecibida}).",
                ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.Motivo))
            return Result<DevolucionResponse>.Failure("El motivo de la devolución es obligatorio.", ErrorType.Validation);

        var devolucion = new DevolucionProveedor
        {
            PedidoProveedorId     = id,
            PedidoProveedorItemId = item.Id,
            Cantidad              = request.Cantidad,
            Motivo                = request.Motivo.Trim(),
        };

        db.DevolucionesProveedor.Add(devolucion);

        item.Producto.StockActual -= request.Cantidad;
        item.Producto.UpdatedAt   =  DateTime.UtcNow;

        db.MovimientosStock.Add(new MovimientoStock
        {
            ProductoId = item.ProductoId,
            Tipo       = "Salida",
            Cantidad   = request.Cantidad,
            Motivo     = $"Devolución proveedor — OC #{pedido.Id}: {request.Motivo.Trim()}",
        });

        await db.SaveChangesAsync();

        return Result<DevolucionResponse>.Success(new DevolucionResponse
        {
            Id             = devolucion.Id,
            ItemId         = item.Id,
            ProductoNombre = item.Producto.Nombre,
            Cantidad       = devolucion.Cantidad,
            Motivo         = devolucion.Motivo,
            CreatedAt      = devolucion.CreatedAt,
        });
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Listado
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<Result<PagedResult<PedidoResponse>>> GetPedidosAsync(
        int? proveedorId, string? estado, int page, int pageSize)
    {
        var query = db.PedidosProveedor.AsQueryable();

        if (proveedorId.HasValue)
            query = query.Where(p => p.ProveedorId == proveedorId.Value);

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoPedido>(estado, true, out var estadoEnum))
            query = query.Where(p => p.Estado == estadoEnum);

        var totalCount = await query.CountAsync();

        var pedidos = await query
            .Include(p => p.Proveedor)
            .Include(p => p.Items).ThenInclude(i => i.Producto)
            .Include(p => p.Devoluciones).ThenInclude(d => d.Item).ThenInclude(i => i.Producto)
            .Include(p => p.Recepciones).ThenInclude(r => r.Items).ThenInclude(ri => ri.PedidoItem).ThenInclude(pi => pi.Producto)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var ids = pedidos.Select(p => p.Id).ToList();
        var facturas = await db.FacturasCompra
            .Where(f => f.PedidoProveedorId != null && ids.Contains(f.PedidoProveedorId.Value))
            .ToListAsync();

        var facturasByPedido = facturas.ToDictionary(f => f.PedidoProveedorId!.Value);

        return Result<PagedResult<PedidoResponse>>.Success(new PagedResult<PedidoResponse>
        {
            Items      = pedidos.Select(p => ToResponse(p, facturasByPedido.GetValueOrDefault(p.Id))).ToList(),
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        });
    }

    public async Task<Result<PedidoResponse>> GetPedidoByIdAsync(int id)
    {
        var pedido = await db.PedidosProveedor
            .Include(p => p.Proveedor)
            .Include(p => p.Items).ThenInclude(i => i.Producto)
            .Include(p => p.Devoluciones).ThenInclude(d => d.Item).ThenInclude(i => i.Producto)
            .Include(p => p.Recepciones).ThenInclude(r => r.Items).ThenInclude(ri => ri.PedidoItem).ThenInclude(pi => pi.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
            return Result<PedidoResponse>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        var factura = await db.FacturasCompra.FirstOrDefaultAsync(f => f.PedidoProveedorId == id);

        return Result<PedidoResponse>.Success(ToResponse(pedido, factura));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Cancelar
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<Result<PedidoResponse>> CancelarPedidoAsync(int id)
    {
        var pedido = await db.PedidosProveedor.FindAsync(id);
        if (pedido is null)
            return Result<PedidoResponse>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (pedido.Estado == EstadoPedido.RecibidaTotal)
            return Result<PedidoResponse>.Failure("No se puede cancelar un pedido ya recibido en su totalidad.", ErrorType.Validation);

        if (pedido.Estado == EstadoPedido.Cancelada)
            return Result<PedidoResponse>.Failure("El pedido ya está cancelado.", ErrorType.Validation);

        pedido.Estado    = EstadoPedido.Cancelada;
        pedido.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Result<PedidoResponse>.Success(await LoadAndMapAsync(id));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────────

    private async Task<PedidoResponse> LoadAndMapAsync(int id)
    {
        var pedido = await db.PedidosProveedor
            .Include(p => p.Proveedor)
            .Include(p => p.Items).ThenInclude(i => i.Producto)
            .Include(p => p.Devoluciones).ThenInclude(d => d.Item).ThenInclude(i => i.Producto)
            .Include(p => p.Recepciones).ThenInclude(r => r.Items).ThenInclude(ri => ri.PedidoItem).ThenInclude(pi => pi.Producto)
            .FirstAsync(p => p.Id == id);

        var factura = await db.FacturasCompra.FirstOrDefaultAsync(f => f.PedidoProveedorId == id);

        return ToResponse(pedido, factura);
    }

    private static PedidoResponse ToResponse(PedidoProveedor p, FacturaCompra? factura) => new()
    {
        Id              = p.Id,
        ProveedorId     = p.ProveedorId,
        ProveedorNombre = p.Proveedor.Nombre,
        Estado          = p.Estado.ToString(),
        Observaciones   = p.Observaciones,
        CreatedAt       = p.CreatedAt,
        UpdatedAt       = p.UpdatedAt,
        Items = p.Items.Select(i => new PedidoItemResponse
        {
            Id               = i.Id,
            ProductoId       = i.ProductoId,
            ProductoNombre   = i.Producto.Nombre,
            Cantidad         = i.Cantidad,
            CantidadRecibida = i.CantidadRecibida,
            PrecioUnitario   = i.PrecioUnitario,
        }),
        Devoluciones = p.Devoluciones.Select(d => new DevolucionResponse
        {
            Id             = d.Id,
            ItemId         = d.PedidoProveedorItemId,
            ProductoNombre = d.Item.Producto.Nombre,
            Cantidad       = d.Cantidad,
            Motivo         = d.Motivo,
            CreatedAt      = d.CreatedAt,
        }),
        Recepciones = p.Recepciones.OrderByDescending(r => r.CreatedAt).Select(r => new RecepcionComprasResponse
        {
            Id            = r.Id,
            Observaciones = r.Observaciones,
            CreatedAt     = r.CreatedAt,
            Items         = r.Items.Select(ri => new RecepcionComprasItemResponse
            {
                PedidoItemId   = ri.PedidoItemId,
                ProductoNombre = ri.PedidoItem.Producto.Nombre,
                Cantidad       = ri.Cantidad,
            }),
        }),
        Factura = factura is null ? null : new PedidoFacturaResponse
        {
            Id               = factura.Id,
            NroFactura       = factura.NroFactura,
            MontoExento      = factura.MontoExento,
            MontoGravado5    = factura.MontoGravado5,
            MontoGravado10   = factura.MontoGravado10,
            MontoTotal       = factura.MontoTotal,
            Iva5             = factura.Iva5,
            Iva10            = factura.Iva10,
            CondicionVenta   = factura.CondicionVenta.ToString(),
            Estado           = factura.Estado.ToString(),
            FechaEmision     = factura.FechaEmision.ToString("yyyy-MM-dd"),
            FechaVencimiento = factura.FechaVencimiento?.ToString("yyyy-MM-dd"),
            FechaPago        = factura.FechaPago?.ToString("yyyy-MM-dd"),
            Observaciones    = factura.Observaciones,
        },
    };
}
