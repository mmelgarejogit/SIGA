using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Compras;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class FacturasCompraService(AppDbContext db) : IFacturasCompraService
{
    // ─────────────────────────────────────────────────────────────────────────────
    // Listado con filtros
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<Result<PagedResult<FacturaCompraResponse>>> GetFacturasAsync(
        int? proveedorId,
        string? condicionVenta,
        string? estado,
        string? origen,
        string? fechaDesde,
        string? fechaHasta,
        string? search,
        int page,
        int pageSize)
    {
        var query = db.FacturasCompra.Include(f => f.Proveedor).AsQueryable();

        if (proveedorId.HasValue)
            query = query.Where(f => f.ProveedorId == proveedorId.Value);

        if (!string.IsNullOrWhiteSpace(condicionVenta)
            && Enum.TryParse<CondicionVenta>(condicionVenta, ignoreCase: true, out var condEnum))
            query = query.Where(f => f.CondicionVenta == condEnum);

        if (!string.IsNullOrWhiteSpace(estado))
        {
            if (estado.Equals("anulada", StringComparison.OrdinalIgnoreCase))
                query = query.Where(f => f.Estado == EstadoEgreso.Anulado);
            else if (estado.Equals("vigente", StringComparison.OrdinalIgnoreCase))
                query = query.Where(f => f.Estado != EstadoEgreso.Anulado);
        }

        if (!string.IsNullOrWhiteSpace(origen))
        {
            if (origen.Equals("ConOC", StringComparison.OrdinalIgnoreCase))
                query = query.Where(f => f.PedidoProveedorId != null);
            else if (origen.Equals("Directa", StringComparison.OrdinalIgnoreCase))
                query = query.Where(f => f.PedidoProveedorId == null);
        }

        if (!string.IsNullOrWhiteSpace(fechaDesde) && DateOnly.TryParse(fechaDesde, out var desde))
            query = query.Where(f => f.FechaEmision >= desde);

        if (!string.IsNullOrWhiteSpace(fechaHasta) && DateOnly.TryParse(fechaHasta, out var hasta))
            query = query.Where(f => f.FechaEmision <= hasta);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(f => EF.Functions.ILike(f.NroFactura ?? "", term));
        }

        var totalCount = await query.CountAsync();

        var facturas = await query
            .OrderByDescending(f => f.FechaEmision)
            .ThenByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Determinar qué OCs (de las facturas paginadas) tienen recepciones
        var pedidoIds = facturas
            .Where(f => f.PedidoProveedorId.HasValue)
            .Select(f => f.PedidoProveedorId!.Value)
            .Distinct()
            .ToList();

        var pedidosConRecepciones = new HashSet<int>();
        if (pedidoIds.Count > 0)
        {
            var ids = await db.RecepcionesMercaderia
                .Where(r => pedidoIds.Contains(r.PedidoProveedorId))
                .Select(r => r.PedidoProveedorId)
                .Distinct()
                .ToListAsync();
            pedidosConRecepciones = ids.ToHashSet();
        }

        return Result<PagedResult<FacturaCompraResponse>>.Success(new PagedResult<FacturaCompraResponse>
        {
            Items      = facturas.Select(f => ToResponse(f, pedidosConRecepciones)).ToList(),
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
        });
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Detalle por ID
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<Result<FacturaCompraResponse>> GetFacturaByIdAsync(int id)
    {
        var factura = await db.FacturasCompra
            .Include(f => f.Proveedor)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (factura is null)
            return Result<FacturaCompraResponse>.Failure("Factura no encontrada.", ErrorType.NotFound);

        var tieneRecepciones = factura.PedidoProveedorId.HasValue
            && await db.RecepcionesMercaderia.AnyAsync(r => r.PedidoProveedorId == factura.PedidoProveedorId.Value);

        return Result<FacturaCompraResponse>.Success(ToResponse(factura, tieneRecepciones));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Registrar factura de compra directa (sin OC)
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<Result<FacturaCompraResponse>> RegistrarFacturaDirectaAsync(RegistrarFacturaDirectaRequest request)
    {
        var proveedor = await db.Proveedores.FindAsync(request.ProveedorId);
        if (proveedor is null)
            return Result<FacturaCompraResponse>.Failure("Proveedor no encontrado.", ErrorType.NotFound);

        var nroFactura = request.NroFactura.Trim();
        var duplicado = await db.FacturasCompra.AnyAsync(f =>
            f.ProveedorId == request.ProveedorId &&
            f.NroFactura  == nroFactura);
        if (duplicado)
            return Result<FacturaCompraResponse>.Failure(
                $"La factura {nroFactura} ya fue registrada para este proveedor.",
                ErrorType.Conflict);

        if (!DateOnly.TryParse(request.FechaEmision, out var fechaEmision))
            return Result<FacturaCompraResponse>.Failure("Fecha de emisión inválida.", ErrorType.Validation);

        DateOnly? fechaVencimiento = null;
        if (!string.IsNullOrWhiteSpace(request.FechaVencimiento))
        {
            if (!DateOnly.TryParse(request.FechaVencimiento, out var fv))
                return Result<FacturaCompraResponse>.Failure("Fecha de vencimiento inválida.", ErrorType.Validation);
            fechaVencimiento = fv;
        }

        if (!Enum.TryParse<CondicionVenta>(request.CondicionVenta, ignoreCase: true, out var condicion))
            return Result<FacturaCompraResponse>.Failure(
                "Condición de venta inválida. Valores: Contado, Credito.",
                ErrorType.Validation);

        var factura = new FacturaCompra
        {
            ProveedorId      = request.ProveedorId,
            PedidoProveedorId = null,            // compra directa
            NroFactura       = nroFactura,
            MontoExento      = request.MontoExento,
            MontoGravado5    = request.MontoGravado5,
            MontoGravado10   = request.MontoGravado10,
            CondicionVenta   = condicion,
            Concepto         = $"Factura de compra directa — {nroFactura} — {proveedor.Nombre}",
            Observaciones    = request.Observaciones?.Trim(),
            FechaEmision     = fechaEmision,
            FechaVencimiento = fechaVencimiento,
            Estado           = EstadoEgreso.Pendiente,
        };
        factura.Monto = factura.MontoTotal;

        db.FacturasCompra.Add(factura);
        await db.SaveChangesAsync();

        // Recargar con Proveedor
        await db.Entry(factura).Reference(f => f.Proveedor).LoadAsync();

        return Result<FacturaCompraResponse>.Success(ToResponse(factura, false));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Anular factura
    // ─────────────────────────────────────────────────────────────────────────────

    public async Task<Result<FacturaCompraResponse>> AnularFacturaAsync(int id, AnularFacturaRequest request)
    {
        var factura = await db.FacturasCompra
            .Include(f => f.Proveedor)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (factura is null)
            return Result<FacturaCompraResponse>.Failure("Factura no encontrada.", ErrorType.NotFound);

        if (factura.Estado == EstadoEgreso.Anulado)
            return Result<FacturaCompraResponse>.Failure("La factura ya está anulada.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.Motivo))
            return Result<FacturaCompraResponse>.Failure("El motivo de anulación es obligatorio.", ErrorType.Validation);

        // Verificar recepciones (solo aplica si está vinculada a una OC)
        if (factura.PedidoProveedorId.HasValue)
        {
            var tieneRecepciones = await db.RecepcionesMercaderia
                .AnyAsync(r => r.PedidoProveedorId == factura.PedidoProveedorId.Value);

            if (tieneRecepciones)
                return Result<FacturaCompraResponse>.Failure(
                    "No se puede anular la factura porque el pedido tiene recepciones registradas.",
                    ErrorType.Validation);

            // Revertir OC a estado Confirmada
            var pedido = await db.PedidosProveedor.FindAsync(factura.PedidoProveedorId.Value);
            if (pedido is not null && pedido.Estado == EstadoPedido.Facturada)
            {
                pedido.Estado    = EstadoPedido.Confirmada;
                pedido.UpdatedAt = DateTime.UtcNow;
            }
        }

        factura.Estado           = EstadoEgreso.Anulado;
        factura.MotivoAnulacion  = request.Motivo.Trim();
        factura.UpdatedAt        = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return Result<FacturaCompraResponse>.Success(ToResponse(factura, false));
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Helper
    // ─────────────────────────────────────────────────────────────────────────────

    private static FacturaCompraResponse ToResponse(FacturaCompra f, bool tieneRecepciones) => new()
    {
        Id                = f.Id,
        NroFactura        = f.NroFactura,
        ProveedorId       = f.ProveedorId,
        ProveedorNombre   = f.Proveedor.Nombre,
        PedidoProveedorId = f.PedidoProveedorId,
        MontoExento       = f.MontoExento,
        MontoGravado5     = f.MontoGravado5,
        MontoGravado10    = f.MontoGravado10,
        MontoTotal        = f.MontoTotal,
        Iva5              = f.Iva5,
        Iva10             = f.Iva10,
        CondicionVenta    = f.CondicionVenta.ToString(),
        Estado            = f.Estado.ToString(),
        FechaEmision      = f.FechaEmision.ToString("yyyy-MM-dd"),
        FechaVencimiento  = f.FechaVencimiento?.ToString("yyyy-MM-dd"),
        FechaPago         = f.FechaPago?.ToString("yyyy-MM-dd"),
        Observaciones     = f.Observaciones,
        MotivoAnulacion   = f.MotivoAnulacion,
        CreatedAt         = f.CreatedAt,
        TieneRecepciones  = tieneRecepciones,
    };

    private static FacturaCompraResponse ToResponse(FacturaCompra f, HashSet<int> pedidosConRecepciones)
        => ToResponse(f, f.PedidoProveedorId.HasValue && pedidosConRecepciones.Contains(f.PedidoProveedorId.Value));
}
