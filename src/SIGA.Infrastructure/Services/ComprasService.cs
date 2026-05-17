using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Compras;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class ComprasService(AppDbContext db) : IComprasService
{
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

    public async Task<Result<PedidoResponse>> EmitirPedidoAsync(int id)
    {
        var pedido = await db.PedidosProveedor.FindAsync(id);
        if (pedido is null)
            return Result<PedidoResponse>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (pedido.Estado != EstadoPedido.Borrador)
            return Result<PedidoResponse>.Failure("Solo se pueden emitir pedidos en estado Borrador.", ErrorType.Validation);

        pedido.Estado    = EstadoPedido.Emitida;
        pedido.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Result<PedidoResponse>.Success(await LoadAndMapAsync(id));
    }

    public async Task<Result<PedidoResponse>> RegistrarRecepcionAsync(int id, RegistrarRecepcionRequest request)
    {
        var pedido = await db.PedidosProveedor
            .Include(p => p.Items).ThenInclude(i => i.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
            return Result<PedidoResponse>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (pedido.Estado != EstadoPedido.Emitida && pedido.Estado != EstadoPedido.ParcialmenteRecibida)
            return Result<PedidoResponse>.Failure(
                "Solo se puede registrar recepción en pedidos Emitidos o Parcialmente Recibidos.",
                ErrorType.Validation);

        var recepciones = request.Items.ToList();
        if (recepciones.Count == 0)
            return Result<PedidoResponse>.Failure("Debe indicar al menos un ítem a recibir.", ErrorType.Validation);

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

            item.CantidadRecibida += rec.CantidadRecibida;

            item.Producto.StockActual += rec.CantidadRecibida;
            item.Producto.UpdatedAt   =  DateTime.UtcNow;

            db.MovimientosStock.Add(new MovimientoStock
            {
                ProductoId = item.ProductoId,
                Tipo       = "Entrada",
                Cantidad   = rec.CantidadRecibida,
                Motivo     = $"Recepción de pedido #{pedido.Id}",
            });
        }

        pedido.Estado    = pedido.Items.All(i => i.CantidadRecibida >= i.Cantidad)
            ? EstadoPedido.Recibida
            : EstadoPedido.ParcialmenteRecibida;
        pedido.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Result<PedidoResponse>.Success(await LoadAndMapAsync(id));
    }

    public async Task<Result<DevolucionResponse>> RegistrarDevolucionAsync(int id, RegistrarDevolucionRequest request)
    {
        var pedido = await db.PedidosProveedor
            .Include(p => p.Items).ThenInclude(i => i.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
            return Result<DevolucionResponse>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (pedido.Estado != EstadoPedido.Recibida && pedido.Estado != EstadoPedido.ParcialmenteRecibida)
            return Result<DevolucionResponse>.Failure(
                "Solo se pueden registrar devoluciones en pedidos Recibidos o Parcialmente Recibidos.",
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
            Motivo     = $"Devolución a proveedor — pedido #{pedido.Id}: {request.Motivo.Trim()}",
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
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<PedidoResponse>>.Success(new PagedResult<PedidoResponse>
        {
            Items      = pedidos.Select(ToResponse).ToList(),
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
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
            return Result<PedidoResponse>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        return Result<PedidoResponse>.Success(ToResponse(pedido));
    }

    public async Task<Result<PedidoResponse>> CancelarPedidoAsync(int id)
    {
        var pedido = await db.PedidosProveedor.FindAsync(id);
        if (pedido is null)
            return Result<PedidoResponse>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (pedido.Estado == EstadoPedido.Recibida)
            return Result<PedidoResponse>.Failure("No se puede cancelar un pedido ya recibido.", ErrorType.Validation);

        if (pedido.Estado == EstadoPedido.Cancelada)
            return Result<PedidoResponse>.Failure("El pedido ya está cancelado.", ErrorType.Validation);

        pedido.Estado    = EstadoPedido.Cancelada;
        pedido.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Result<PedidoResponse>.Success(await LoadAndMapAsync(id));
    }

    private async Task<PedidoResponse> LoadAndMapAsync(int id)
    {
        var pedido = await db.PedidosProveedor
            .Include(p => p.Proveedor)
            .Include(p => p.Items).ThenInclude(i => i.Producto)
            .Include(p => p.Devoluciones).ThenInclude(d => d.Item).ThenInclude(i => i.Producto)
            .FirstAsync(p => p.Id == id);

        return ToResponse(pedido);
    }

    private static PedidoResponse ToResponse(PedidoProveedor p) => new()
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
    };
}
