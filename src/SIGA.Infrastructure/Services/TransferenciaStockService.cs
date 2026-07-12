using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Sucursales;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class TransferenciaStockService(AppDbContext db, ICurrentUserContext current, INotificacionInternaService notificacion) : ITransferenciaStockService
{
    public async Task<Result<IEnumerable<TransferenciaResponse>>> GetAllAsync(string? estado = null)
    {
        var query = db.TransferenciasStock
            .Include(t => t.SucursalOrigen)
            .Include(t => t.SucursalDestino)
            .Include(t => t.Items).ThenInclude(i => i.Producto)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoTransferenciaStock>(estado, ignoreCase: true, out var estadoFiltro))
            query = query.Where(t => t.Estado == estadoFiltro);

        var items = await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
        return Result<IEnumerable<TransferenciaResponse>>.Success(items.Select(ToResponse));
    }

    public async Task<Result<TransferenciaResponse>> CreateAsync(CreateTransferenciaRequest request, int userId, string userName)
    {
        if (request.Items is null || request.Items.Count == 0)
            return Result<TransferenciaResponse>.Failure("La transferencia debe tener al menos un producto.", ErrorType.Validation);

        if (request.Items.Any(i => i.Cantidad <= 0))
            return Result<TransferenciaResponse>.Failure("Las cantidades deben ser mayores a cero.", ErrorType.Validation);

        // Origen: la sucursal del usuario; un admin global puede indicarla.
        var origenId = current.SucursalId ?? request.SucursalOrigenId ?? 0;
        if (origenId == 0)
            return Result<TransferenciaResponse>.Failure("Indicá la sucursal de origen.", ErrorType.Validation);

        if (origenId == request.SucursalDestinoId)
            return Result<TransferenciaResponse>.Failure("La sucursal de origen y destino no pueden ser la misma.", ErrorType.Validation);

        if (!await db.Sucursales.AnyAsync(s => s.Id == origenId && s.IsActive))
            return Result<TransferenciaResponse>.Failure("La sucursal de origen no existe o está inactiva.", ErrorType.Validation);

        if (!await db.Sucursales.AnyAsync(s => s.Id == request.SucursalDestinoId && s.IsActive))
            return Result<TransferenciaResponse>.Failure("La sucursal de destino no existe o está inactiva.", ErrorType.Validation);

        // Consolidar por producto y validar stock disponible en origen.
        var consolidados = request.Items
            .GroupBy(i => i.ProductoId)
            .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
            .ToList();

        var productoIds = consolidados.Select(c => c.ProductoId).ToList();
        var productos = await db.Productos.Where(p => productoIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p);

        var stockMap = await db.StockActual
            .Where(s => productoIds.Contains(s.ProductoId) && s.SucursalId == origenId)
            .ToDictionaryAsync(s => s.ProductoId, s => s.StockActual);

        var insuficientes = new List<string>();
        foreach (var c in consolidados)
        {
            if (!productos.ContainsKey(c.ProductoId))
                return Result<TransferenciaResponse>.Failure($"Producto #{c.ProductoId} no encontrado.", ErrorType.NotFound);
            var disponible = stockMap.GetValueOrDefault(c.ProductoId, 0);
            if (disponible < c.Cantidad)
                insuficientes.Add($"\"{productos[c.ProductoId].Nombre}\" (disponible: {disponible}, solicitado: {c.Cantidad})");
        }
        if (insuficientes.Count > 0)
            return Result<TransferenciaResponse>.Failure(
                "Stock insuficiente en la sucursal de origen: " + string.Join("; ", insuficientes), ErrorType.Validation);

        var now = DateTime.UtcNow;
        var transferencia = new TransferenciaStock
        {
            SucursalOrigenId  = origenId,
            SucursalDestinoId = request.SucursalDestinoId,
            Fecha             = DateOnly.FromDateTime(now),
            Estado            = EstadoTransferenciaStock.Pendiente,
            CreadoPorId       = userId.ToString(),
            CreadoPorNombre   = userName,
            Observaciones     = request.Observaciones?.Trim(),
            CreatedAt         = now,
            Items = consolidados.Select(c => new TransferenciaStockItem
            {
                ProductoId = c.ProductoId,
                Cantidad   = c.Cantidad,
            }).ToList(),
        };

        db.TransferenciasStock.Add(transferencia);

        // El stock SALE del origen al crear la transferencia (queda en tránsito).
        foreach (var c in consolidados)
        {
            db.MovimientosStock.Add(new MovimientoStock
            {
                ProductoId      = c.ProductoId,
                SucursalId      = origenId,
                Tipo            = TipoMovimientoStock.Salida,
                Cantidad        = c.Cantidad,
                Motivo          = $"Transferencia a sucursal #{request.SucursalDestinoId}",
                Estado          = EstadoMovimientoStock.Aprobado,
                FechaMovimiento = now,
                FechaAprobacion = now,
                CreadoPorNombre = userName,
            });
        }

        await db.SaveChangesAsync();

        await notificacion.CrearAsync(
            tipo: TipoNotificacion.TransferenciaPendiente,
            mensaje: $"Nueva transferencia de stock #{transferencia.Id} pendiente de aprobación.",
            entidadOrigenTipo: "TransferenciaStock",
            entidadOrigenId: transferencia.Id,
            destinatarioSucursalId: transferencia.SucursalDestinoId);

        return await GetByIdAsync(transferencia.Id);
    }

    public async Task<Result<TransferenciaResponse>> GestionarAsync(int id, GestionarTransferenciaRequest request, string userName)
    {
        var t = await db.TransferenciasStock
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (t is null)
            return Result<TransferenciaResponse>.Failure("Transferencia no encontrada.", ErrorType.NotFound);

        if (t.Estado != EstadoTransferenciaStock.Pendiente)
            return Result<TransferenciaResponse>.Failure("La transferencia ya fue gestionada.", ErrorType.Conflict);

        // Solo la sucursal de destino (o un admin global) acepta/rechaza.
        if (current.SucursalId is int b && b != t.SucursalDestinoId)
            return Result<TransferenciaResponse>.Failure("Solo la sucursal de destino puede gestionar esta transferencia.", ErrorType.Validation);

        var now = DateTime.UtcNow;

        if (request.Aceptar)
        {
            // El stock INGRESA al destino.
            foreach (var item in t.Items)
            {
                db.MovimientosStock.Add(new MovimientoStock
                {
                    ProductoId      = item.ProductoId,
                    SucursalId      = t.SucursalDestinoId,
                    Tipo            = TipoMovimientoStock.Entrada,
                    Cantidad        = item.Cantidad,
                    Motivo          = $"Transferencia recibida desde sucursal #{t.SucursalOrigenId}",
                    Estado          = EstadoMovimientoStock.Aprobado,
                    FechaMovimiento = now,
                    FechaAprobacion = now,
                    CreadoPorNombre = userName,
                });
            }
            t.Estado = EstadoTransferenciaStock.Aceptada;
        }
        else
        {
            // Rechazo: el stock VUELVE al origen (revierte la salida inicial).
            foreach (var item in t.Items)
            {
                db.MovimientosStock.Add(new MovimientoStock
                {
                    ProductoId      = item.ProductoId,
                    SucursalId      = t.SucursalOrigenId,
                    Tipo            = TipoMovimientoStock.Entrada,
                    Cantidad        = item.Cantidad,
                    Motivo          = $"Transferencia #{t.Id} rechazada — devolución al origen",
                    Estado          = EstadoMovimientoStock.Aprobado,
                    FechaMovimiento = now,
                    FechaAprobacion = now,
                    CreadoPorNombre = userName,
                });
            }
            t.Estado = EstadoTransferenciaStock.Rechazada;
        }

        t.RecibidoPorNombre = userName;
        t.FechaResolucion   = now;
        if (!string.IsNullOrWhiteSpace(request.Observaciones))
            t.Observaciones = (t.Observaciones is null ? "" : t.Observaciones + " | ") + request.Observaciones.Trim();

        await db.SaveChangesAsync();
        return await GetByIdAsync(t.Id);
    }

    private async Task<Result<TransferenciaResponse>> GetByIdAsync(int id)
    {
        var t = await db.TransferenciasStock
            .Include(x => x.SucursalOrigen)
            .Include(x => x.SucursalDestino)
            .Include(x => x.Items).ThenInclude(i => i.Producto)
            .FirstAsync(x => x.Id == id);
        return Result<TransferenciaResponse>.Success(ToResponse(t));
    }

    private static TransferenciaResponse ToResponse(TransferenciaStock t) => new()
    {
        Id                    = t.Id,
        SucursalOrigenId      = t.SucursalOrigenId,
        SucursalOrigenNombre  = t.SucursalOrigen?.Nombre ?? "",
        SucursalDestinoId     = t.SucursalDestinoId,
        SucursalDestinoNombre = t.SucursalDestino?.Nombre ?? "",
        Fecha                 = t.Fecha.ToString("yyyy-MM-dd"),
        Estado                = t.Estado.ToString(),
        CreadoPorNombre       = t.CreadoPorNombre,
        RecibidoPorNombre     = t.RecibidoPorNombre,
        Observaciones         = t.Observaciones,
        CreatedAt             = t.CreatedAt,
        Items = t.Items.Select(i => new TransferenciaItemResponse
        {
            ProductoId     = i.ProductoId,
            ProductoNombre = i.Producto?.Nombre ?? "",
            Cantidad       = i.Cantidad,
        }).ToList(),
    };
}
