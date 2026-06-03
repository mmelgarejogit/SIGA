using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Compras;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class RecepcionesService(AppDbContext db) : IRecepcionesService
{
    // ─────────────────────────────────────────────────────────────────────────
    // Listado de recepciones con filtros
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<Result<PagedResult<RecepcionListResponse>>> GetRecepcionesAsync(
        int? proveedorId,
        string? estadoOC,
        string? fechaDesde,
        string? fechaHasta,
        int page,
        int pageSize)
    {
        var query = db.RecepcionesMercaderia
            .Include(r => r.PedidoProveedor).ThenInclude(p => p.Proveedor)
            .Include(r => r.FacturaCompra)
            .Include(r => r.User).ThenInclude(u => u.Person)
            .Include(r => r.Items)
            .AsQueryable();

        if (proveedorId.HasValue)
            query = query.Where(r => r.PedidoProveedor.ProveedorId == proveedorId.Value);

        if (!string.IsNullOrWhiteSpace(estadoOC)
            && Enum.TryParse<EstadoPedido>(estadoOC, ignoreCase: true, out var estadoEnum))
            query = query.Where(r => r.PedidoProveedor.Estado == estadoEnum);

        if (!string.IsNullOrWhiteSpace(fechaDesde) && DateOnly.TryParse(fechaDesde, out var desde))
            query = query.Where(r => r.FechaRecepcion >= desde);

        if (!string.IsNullOrWhiteSpace(fechaHasta) && DateOnly.TryParse(fechaHasta, out var hasta))
            query = query.Where(r => r.FechaRecepcion <= hasta);

        var total = await query.CountAsync();

        var recepciones = await query
            .OrderByDescending(r => r.FechaRecepcion)
            .ThenByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<RecepcionListResponse>>.Success(new PagedResult<RecepcionListResponse>
        {
            Items      = recepciones.Select(ToListResponse).ToList(),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Facturas habilitadas para recepción
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<Result<List<FacturaDisponibleResponse>>> GetFacturasDisponiblesAsync()
    {
        var facturas = await db.FacturasCompra
            .Include(f => f.Proveedor)
            .Where(f =>
                f.PedidoProveedorId != null &&
                f.Estado != EstadoEgreso.Anulado &&
                f.PedidoProveedor!.Estado != EstadoPedido.Cancelada &&
                f.PedidoProveedor.Estado != EstadoPedido.RecibidaTotal &&
                (f.PedidoProveedor.Estado == EstadoPedido.Facturada
                 || f.PedidoProveedor.Estado == EstadoPedido.RecibidaParcial))
            .OrderByDescending(f => f.FechaEmision)
            .Select(f => new FacturaDisponibleResponse
            {
                Id                = f.Id,
                NroFactura        = f.NroFactura ?? "",
                FechaEmision      = f.FechaEmision.ToString("yyyy-MM-dd"),
                PedidoProveedorId = f.PedidoProveedorId!.Value,
                EstadoOC          = f.PedidoProveedor!.Estado.ToString(),
                ProveedorId       = f.ProveedorId,
                ProveedorNombre   = f.Proveedor.Nombre,
                ItemsPendientes   = f.PedidoProveedor.Items.Count(i => i.Cantidad > i.CantidadRecibida),
            })
            .ToListAsync();

        // Filtrar las que ya no tienen ítems pendientes
        facturas = facturas.Where(f => f.ItemsPendientes > 0).ToList();

        return Result<List<FacturaDisponibleResponse>>.Success(facturas);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Registrar recepción
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<Result<RecepcionListResponse>> RegistrarRecepcionAsync(
        int userId, RegistrarRecepcionRequest request)
    {
        var factura = await db.FacturasCompra
            .Include(f => f.Proveedor)
            .Include(f => f.PedidoProveedor!)
                .ThenInclude(p => p.Items).ThenInclude(i => i.Producto)
            .FirstOrDefaultAsync(f => f.Id == request.FacturaCompraId);

        if (factura is null)
            return Result<RecepcionListResponse>.Failure("Factura no encontrada.", ErrorType.NotFound);

        if (factura.Estado == EstadoEgreso.Anulado)
            return Result<RecepcionListResponse>.Failure("No se puede recepcionar una factura anulada.", ErrorType.Validation);

        if (factura.PedidoProveedorId is null || factura.PedidoProveedor is null)
            return Result<RecepcionListResponse>.Failure(
                "Esta factura no tiene OC asociada — no se puede recepcionar.",
                ErrorType.Validation);

        var pedido = factura.PedidoProveedor;
        if (pedido.Estado != EstadoPedido.Facturada && pedido.Estado != EstadoPedido.RecibidaParcial)
            return Result<RecepcionListResponse>.Failure(
                "Solo se puede recepcionar OCs en estado Facturada o Recibida parcial.",
                ErrorType.Validation);

        if (!DateOnly.TryParse(request.FechaRecepcion, out var fechaRecepcion))
            return Result<RecepcionListResponse>.Failure("Fecha de recepción inválida.", ErrorType.Validation);

        var recepciones = request.Items.Where(i => i.CantidadRecibida > 0).ToList();
        if (recepciones.Count == 0)
            return Result<RecepcionListResponse>.Failure(
                "Debe indicar al menos un ítem a recibir.", ErrorType.Validation);

        // Crear la recepción
        var recepcion = new RecepcionMercaderia
        {
            PedidoProveedorId = pedido.Id,
            FacturaCompraId   = factura.Id,
            FechaRecepcion    = fechaRecepcion,
            UserId            = userId,
            Observaciones     = request.Observaciones?.Trim(),
        };

        foreach (var rec in recepciones)
        {
            var item = pedido.Items.FirstOrDefault(i => i.Id == rec.ItemId);
            if (item is null)
                return Result<RecepcionListResponse>.Failure(
                    $"Ítem {rec.ItemId} no pertenece a esta OC.", ErrorType.Validation);

            var pendiente = item.Cantidad - item.CantidadRecibida;
            if (rec.CantidadRecibida > pendiente)
                return Result<RecepcionListResponse>.Failure(
                    $"\"{item.Producto.Nombre}\" excede la cantidad pendiente ({pendiente}).",
                    ErrorType.Validation);

            // Lote + Vencimiento — si hay uno, ambos son obligatorios
            var loteTrim = rec.Lote?.Trim();
            DateOnly? venc = null;
            if (!string.IsNullOrWhiteSpace(rec.FechaVencimiento))
            {
                if (!DateOnly.TryParse(rec.FechaVencimiento, out var v))
                    return Result<RecepcionListResponse>.Failure(
                        $"Fecha de vencimiento inválida para \"{item.Producto.Nombre}\".",
                        ErrorType.Validation);
                venc = v;
            }
            var tieneLote = !string.IsNullOrWhiteSpace(loteTrim);
            var tieneVenc = venc.HasValue;
            if (tieneLote != tieneVenc)
                return Result<RecepcionListResponse>.Failure(
                    $"Para \"{item.Producto.Nombre}\": si se completa lote o vencimiento, ambos son obligatorios.",
                    ErrorType.Validation);

            recepcion.Items.Add(new RecepcionMercaderiaItem
            {
                PedidoItemId     = item.Id,
                Cantidad         = rec.CantidadRecibida,
                Lote             = tieneLote ? loteTrim : null,
                FechaVencimiento = venc,
                Observaciones    = string.IsNullOrWhiteSpace(rec.Observaciones) ? null : rec.Observaciones.Trim(),
            });

            item.CantidadRecibida += rec.CantidadRecibida;
        }

        db.RecepcionesMercaderia.Add(recepcion);

        // Actualizar estado OC
        pedido.Estado = pedido.Items.All(i => i.CantidadRecibida >= i.Cantidad)
            ? EstadoPedido.RecibidaTotal
            : EstadoPedido.RecibidaParcial;
        pedido.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        // Recargar con joins para mapear la respuesta
        var saved = await db.RecepcionesMercaderia
            .Include(r => r.PedidoProveedor).ThenInclude(p => p.Proveedor)
            .Include(r => r.FacturaCompra)
            .Include(r => r.User).ThenInclude(u => u.Person)
            .Include(r => r.Items)
            .FirstAsync(r => r.Id == recepcion.Id);

        return Result<RecepcionListResponse>.Success(ToListResponse(saved));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Detalle de una recepción
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<Result<RecepcionDetalleResponse>> GetByIdAsync(int id)
    {
        var rec = await db.RecepcionesMercaderia
            .Include(x => x.PedidoProveedor).ThenInclude(p => p.Proveedor)
            .Include(x => x.FacturaCompra)
            .Include(x => x.User).ThenInclude(u => u.Person)
            .Include(x => x.Items).ThenInclude(i => i.PedidoItem).ThenInclude(pi => pi.Producto)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (rec is null)
            return Result<RecepcionDetalleResponse>.Failure("Recepción no encontrada.", ErrorType.NotFound);

        var response = new RecepcionDetalleResponse
        {
            Id                = rec.Id,
            FechaRecepcion    = rec.FechaRecepcion.ToString("yyyy-MM-dd"),
            CreatedAt         = rec.CreatedAt,
            PedidoProveedorId = rec.PedidoProveedorId,
            EstadoOC          = rec.PedidoProveedor.Estado.ToString(),
            ProveedorId       = rec.PedidoProveedor.ProveedorId,
            ProveedorNombre   = rec.PedidoProveedor.Proveedor.Nombre,
            FacturaCompraId   = rec.FacturaCompraId,
            NroFactura        = rec.FacturaCompra?.NroFactura,
            UsuarioId         = rec.UserId,
            UsuarioNombre     = rec.User.Person is not null
                                    ? $"{rec.User.Person.FirstName} {rec.User.Person.LastName}".Trim()
                                    : $"Usuario #{rec.UserId}",
            Observaciones     = rec.Observaciones,
            Items             = rec.Items.Select(i => new RecepcionComprasItemResponse
            {
                PedidoItemId     = i.PedidoItemId,
                ProductoNombre   = i.PedidoItem.Producto.Nombre,
                Cantidad         = i.Cantidad,
                Lote             = i.Lote,
                FechaVencimiento = i.FechaVencimiento?.ToString("yyyy-MM-dd"),
                Observaciones    = i.Observaciones,
            }).ToList(),
        };

        return Result<RecepcionDetalleResponse>.Success(response);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────


    private static RecepcionListResponse ToListResponse(RecepcionMercaderia r) => new()
    {
        Id                = r.Id,
        FechaRecepcion    = r.FechaRecepcion.ToString("yyyy-MM-dd"),
        CreatedAt         = r.CreatedAt,
        PedidoProveedorId = r.PedidoProveedorId,
        EstadoOC          = r.PedidoProveedor.Estado.ToString(),
        ProveedorId       = r.PedidoProveedor.ProveedorId,
        ProveedorNombre   = r.PedidoProveedor.Proveedor.Nombre,
        FacturaCompraId   = r.FacturaCompraId,
        NroFactura        = r.FacturaCompra?.NroFactura,
        UsuarioId         = r.UserId,
        UsuarioNombre     = r.User.Person is not null
                                ? $"{r.User.Person.FirstName} {r.User.Person.LastName}".Trim()
                                : $"Usuario #{r.UserId}",
        CantidadItems     = r.Items.Count,
        CantidadTotal     = r.Items.Sum(i => i.Cantidad),
        Observaciones     = r.Observaciones,
    };
}
