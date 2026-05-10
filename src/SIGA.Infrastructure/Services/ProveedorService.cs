using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class ProveedorService(AppDbContext db) : IProveedorService
{
    public async Task<Result<IEnumerable<ProveedorResponse>>> GetAllAsync(string? search)
    {
        var query = db.Proveedores.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(q));
        }

        var proveedores = await query.OrderBy(p => p.Nombre).ToListAsync();
        return Result<IEnumerable<ProveedorResponse>>.Success(proveedores.Select(ToResponse));
    }

    public async Task<Result<ProveedorResponse>> CreateAsync(CreateProveedorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<ProveedorResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        var proveedor = new Proveedor
        {
            Nombre   = request.Nombre.Trim(),
            Contacto = request.Contacto?.Trim(),
            Email    = request.Email?.Trim(),
            Telefono = request.Telefono?.Trim(),
        };

        db.Proveedores.Add(proveedor);
        await db.SaveChangesAsync();
        return Result<ProveedorResponse>.Success(ToResponse(proveedor));
    }

    public async Task<Result<ProveedorResponse>> UpdateAsync(int id, CreateProveedorRequest request)
    {
        var proveedor = await db.Proveedores.FindAsync(id);
        if (proveedor is null)
            return Result<ProveedorResponse>.Failure("Proveedor no encontrado.", ErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<ProveedorResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        proveedor.Nombre    = request.Nombre.Trim();
        proveedor.Contacto  = request.Contacto?.Trim();
        proveedor.Email     = request.Email?.Trim();
        proveedor.Telefono  = request.Telefono?.Trim();
        proveedor.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Result<ProveedorResponse>.Success(ToResponse(proveedor));
    }

    public async Task<Result<bool>> DeactivateAsync(int id)
    {
        var proveedor = await db.Proveedores.FindAsync(id);
        if (proveedor is null)
            return Result<bool>.Failure("Proveedor no encontrado.", ErrorType.NotFound);

        proveedor.IsActive  = false;
        proveedor.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    public async Task<Result<PedidoProveedorResponse>> CreatePedidoAsync(CreatePedidoProveedorRequest request)
    {
        var proveedor = await db.Proveedores.FindAsync(request.ProveedorId);
        if (proveedor is null)
            return Result<PedidoProveedorResponse>.Failure("Proveedor no encontrado.", ErrorType.NotFound);

        var items = request.Items.ToList();
        if (items.Count == 0)
            return Result<PedidoProveedorResponse>.Failure("El pedido debe tener al menos un ítem.", ErrorType.Validation);

        foreach (var item in items)
        {
            if (item.Cantidad <= 0)
                return Result<PedidoProveedorResponse>.Failure("La cantidad de cada ítem debe ser mayor a cero.", ErrorType.Validation);

            var existe = await db.Productos.AnyAsync(p => p.Id == item.ProductoId);
            if (!existe)
                return Result<PedidoProveedorResponse>.Failure($"Producto {item.ProductoId} no encontrado.", ErrorType.NotFound);
        }

        var pedido = new PedidoProveedor
        {
            ProveedorId    = request.ProveedorId,
            Observaciones  = request.Observaciones?.Trim(),
            Items          = items.Select(i => new PedidoProveedorItem
            {
                ProductoId    = i.ProductoId,
                Cantidad      = i.Cantidad,
                PrecioUnitario = i.PrecioUnitario,
            }).ToList(),
        };

        db.PedidosProveedor.Add(pedido);
        await db.SaveChangesAsync();

        await db.Entry(pedido).Reference(p => p.Proveedor).LoadAsync();
        foreach (var item in pedido.Items)
            await db.Entry(item).Reference(i => i.Producto).LoadAsync();

        return Result<PedidoProveedorResponse>.Success(ToResponse(pedido));
    }

    public async Task<Result<IEnumerable<PedidoProveedorResponse>>> GetPedidosAsync(
        int? proveedorId, string? estado)
    {
        var query = db.PedidosProveedor
            .Include(p => p.Proveedor)
            .Include(p => p.Items).ThenInclude(i => i.Producto)
            .AsQueryable();

        if (proveedorId.HasValue)
            query = query.Where(p => p.ProveedorId == proveedorId.Value);

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(p => p.Estado == estado);

        var pedidos = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
        return Result<IEnumerable<PedidoProveedorResponse>>.Success(pedidos.Select(ToResponse));
    }

    public async Task<Result<PedidoProveedorResponse>> UpdatePedidoEstadoAsync(int id, string estado)
    {
        var pedido = await db.PedidosProveedor
            .Include(p => p.Proveedor)
            .Include(p => p.Items).ThenInclude(i => i.Producto)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
            return Result<PedidoProveedorResponse>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        var estadosValidos = new[] { "Pendiente", "Enviado", "Recibido", "Cancelado" };
        if (!estadosValidos.Contains(estado))
            return Result<PedidoProveedorResponse>.Failure("Estado inválido.", ErrorType.Validation);

        // Al recibir un pedido, actualizar el stock de los productos
        if (estado == "Recibido" && pedido.Estado != "Recibido")
        {
            foreach (var item in pedido.Items)
            {
                var producto = await db.Productos.FindAsync(item.ProductoId);
                if (producto is not null)
                {
                    producto.StockActual += item.Cantidad;
                    producto.UpdatedAt = DateTime.UtcNow;

                    db.MovimientosStock.Add(new MovimientoStock
                    {
                        ProductoId = item.ProductoId,
                        Tipo       = "Entrada",
                        Cantidad   = item.Cantidad,
                        Motivo     = $"Recepción de pedido #{pedido.Id}",
                    });
                }
            }
        }

        pedido.Estado    = estado;
        pedido.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Result<PedidoProveedorResponse>.Success(ToResponse(pedido));
    }

    public async Task<Result<bool>> CancelPedidoAsync(int id)
    {
        var pedido = await db.PedidosProveedor.FindAsync(id);
        if (pedido is null)
            return Result<bool>.Failure("Pedido no encontrado.", ErrorType.NotFound);

        if (pedido.Estado == "Recibido")
            return Result<bool>.Failure("No se puede cancelar un pedido ya recibido.", ErrorType.Validation);

        pedido.Estado    = "Cancelado";
        pedido.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static ProveedorResponse ToResponse(Proveedor p) => new()
    {
        Id        = p.Id,
        Nombre    = p.Nombre,
        Contacto  = p.Contacto,
        Email     = p.Email,
        Telefono  = p.Telefono,
        IsActive  = p.IsActive,
        CreatedAt = p.CreatedAt,
    };

    private static PedidoProveedorResponse ToResponse(PedidoProveedor p) => new()
    {
        Id              = p.Id,
        ProveedorId     = p.ProveedorId,
        ProveedorNombre = p.Proveedor.Nombre,
        Estado          = p.Estado,
        Observaciones   = p.Observaciones,
        CreatedAt       = p.CreatedAt,
        UpdatedAt       = p.UpdatedAt,
        Items           = p.Items.Select(i => new PedidoProveedorItemResponse
        {
            Id             = i.Id,
            ProductoId     = i.ProductoId,
            ProductoNombre = i.Producto.Nombre,
            Cantidad       = i.Cantidad,
            PrecioUnitario = i.PrecioUnitario,
        }),
    };
}
