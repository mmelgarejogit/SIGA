using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class StockLoteService(AppDbContext db) : IStockLoteService
{
    // ── Listar lotes ──────────────────────────────────────────────────────────

    public async Task<Result<List<StockLoteDto>>> GetLotesAsync(int? productoId, bool? vencidos)
    {
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        var query = db.StockLotes
            .Include(l => l.Producto).ThenInclude(p => p.Marca)
            .Include(l => l.Producto).ThenInclude(p => p.Modelo)
            .AsQueryable();

        if (productoId.HasValue)
            query = query.Where(l => l.ProductoId == productoId.Value);

        if (vencidos == true)
            query = query.Where(l => l.FechaVencimiento.HasValue && l.FechaVencimiento.Value < hoy);
        else if (vencidos == false)
            query = query.Where(l => !l.FechaVencimiento.HasValue || l.FechaVencimiento.Value >= hoy);

        var lotes = await query
            .OrderBy(l => l.FechaVencimiento)
            .ThenBy(l => l.FechaIngreso)
            .ToListAsync();

        return Result<List<StockLoteDto>>.Success(lotes.Select(ToLoteDto).ToList());
    }

    // ── Registrar conteo por producto (queda Pendiente) ───────────────────────

    public async Task<Result<ConteoInventarioDto>> RegistrarConteoAsync(
        int userId, string userName, RegistrarConteoRequest request)
    {
        if (request.Items.Count == 0)
            return Result<ConteoInventarioDto>.Failure(
                "Debe incluir al menos un producto en el conteo.", ErrorType.Validation);

        var productoIds = request.Items.Select(i => i.ProductoId).Distinct().ToList();
        var productos = await db.Productos
            .Where(p => productoIds.Contains(p.Id))
            .ToListAsync();

        if (productos.Count != productoIds.Count)
            return Result<ConteoInventarioDto>.Failure(
                "Uno o más productos no fueron encontrados.", ErrorType.Validation);

        var conteo = new ConteoInventario
        {
            CreadoPorId     = userId,
            CreadoPorNombre = userName,
            Observaciones   = request.Observaciones?.Trim(),
        };

        foreach (var req in request.Items)
        {
            var producto  = productos.First(p => p.Id == req.ProductoId);
            var diferencia = req.CantidadFisica - producto.StockActual;

            conteo.Lineas.Add(new ConteoInventarioLinea
            {
                ProductoId      = producto.Id,
                CantidadSistema = producto.StockActual,
                CantidadFisica  = req.CantidadFisica,
                Diferencia      = diferencia,
            });
        }

        db.ConteosInventario.Add(conteo);
        await db.SaveChangesAsync();

        return Result<ConteoInventarioDto>.Success(ToDto(conteo));
    }

    // ── Listar conteos ────────────────────────────────────────────────────────

    public async Task<Result<List<ConteoInventarioDto>>> GetConteosAsync(string? estado)
    {
        var query = db.ConteosInventario
            .Include(c => c.Lineas)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
            query = query.Where(c => c.Estado == estado);

        var conteos = await query
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return Result<List<ConteoInventarioDto>>.Success(conteos.Select(ToDto).ToList());
    }

    // ── Detalle de un conteo ──────────────────────────────────────────────────

    public async Task<Result<ConteoInventarioDetalleDto>> GetConteoByIdAsync(int id)
    {
        var conteo = await db.ConteosInventario
            .Include(c => c.Lineas).ThenInclude(l => l.Producto)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (conteo is null)
            return Result<ConteoInventarioDetalleDto>.Failure("Inventario no encontrado.", ErrorType.NotFound);

        return Result<ConteoInventarioDetalleDto>.Success(ToDetalleDto(conteo));
    }

    // ── Aprobar / rechazar conteo ─────────────────────────────────────────────

    public async Task<Result<ConteoInventarioDto>> GestionarConteoAsync(
        int id, int userId, string userName, GestionarConteoRequest request)
    {
        if (request.Accion != "Aprobado" && request.Accion != "Rechazado")
            return Result<ConteoInventarioDto>.Failure(
                "Acción inválida. Use 'Aprobado' o 'Rechazado'.", ErrorType.Validation);

        var conteo = await db.ConteosInventario
            .Include(c => c.Lineas).ThenInclude(l => l.Producto)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (conteo is null)
            return Result<ConteoInventarioDto>.Failure("Inventario no encontrado.", ErrorType.NotFound);

        if (conteo.Estado != "Pendiente")
            return Result<ConteoInventarioDto>.Failure(
                "Solo se pueden gestionar inventarios en estado Pendiente.", ErrorType.Validation);

        conteo.Estado                  = request.Accion;
        conteo.AprobadoPorNombre       = userName;
        conteo.FechaAprobacion         = DateTime.UtcNow;
        conteo.ObservacionesAprobacion = request.Observaciones?.Trim();

        if (request.Accion == "Aprobado")
        {
            foreach (var linea in conteo.Lineas.Where(l => l.Diferencia != 0))
            {
                var tipo = linea.Diferencia > 0 ? "Entrada" : "Salida";

                db.MovimientosStock.Add(new MovimientoStock
                {
                    ProductoId              = linea.ProductoId,
                    Tipo                    = tipo,
                    Cantidad                = Math.Abs(linea.Diferencia),
                    Motivo                  = $"Ajuste de inventario físico — Inventario #{conteo.Id}",
                    CreadoPorId             = userId.ToString(),
                    Estado                  = "Aprobado",
                    AprobadoPorNombre       = userName,
                    FechaAprobacion         = DateTime.UtcNow,
                    ObservacionesAprobacion = request.Observaciones?.Trim(),
                });

                linea.Producto.StockActual += linea.Diferencia;
                linea.Producto.UpdatedAt    = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync();
        return Result<ConteoInventarioDto>.Success(ToDto(conteo));
    }

    // ── Mappers ───────────────────────────────────────────────────────────────

    private static StockLoteDto ToLoteDto(StockLote l) => new()
    {
        Id               = l.Id,
        ProductoId       = l.ProductoId,
        ProductoNombre   = l.Producto.Nombre,
        ProductoSku      = l.Producto.Sku,
        MarcaNombre      = l.Producto.Marca?.Nombre,
        ModeloNombre     = l.Producto.Modelo?.Nombre,
        Lote             = l.Lote,
        FechaVencimiento = l.FechaVencimiento?.ToString("yyyy-MM-dd"),
        CantidadInicial  = l.CantidadInicial,
        FechaIngreso     = l.FechaIngreso.ToString("yyyy-MM-dd"),
        RecepcionItemId  = l.RecepcionItemId,
    };

    private static ConteoInventarioDto ToDto(ConteoInventario c) => new()
    {
        Id                      = c.Id,
        CreadoPorNombre         = c.CreadoPorNombre,
        FechaConteo             = c.FechaConteo.ToString("yyyy-MM-ddTHH:mm:ss"),
        Estado                  = c.Estado,
        Observaciones           = c.Observaciones,
        AprobadoPorNombre       = c.AprobadoPorNombre,
        FechaAprobacion         = c.FechaAprobacion?.ToString("yyyy-MM-ddTHH:mm:ss"),
        ObservacionesAprobacion = c.ObservacionesAprobacion,
        TotalLineas             = c.Lineas.Count,
        LineasConDiferencia     = c.Lineas.Count(l => l.Diferencia != 0),
    };

    private static ConteoInventarioDetalleDto ToDetalleDto(ConteoInventario c) => new()
    {
        Id                      = c.Id,
        CreadoPorNombre         = c.CreadoPorNombre,
        FechaConteo             = c.FechaConteo.ToString("yyyy-MM-ddTHH:mm:ss"),
        Estado                  = c.Estado,
        Observaciones           = c.Observaciones,
        AprobadoPorNombre       = c.AprobadoPorNombre,
        FechaAprobacion         = c.FechaAprobacion?.ToString("yyyy-MM-ddTHH:mm:ss"),
        ObservacionesAprobacion = c.ObservacionesAprobacion,
        TotalLineas             = c.Lineas.Count,
        LineasConDiferencia     = c.Lineas.Count(l => l.Diferencia != 0),
        Lineas                  = c.Lineas.Select(l => new ConteoLineaDto
        {
            Id              = l.Id,
            ProductoId      = l.ProductoId,
            ProductoNombre     = l.Producto.Nombre,
            ProductoSku        = l.Producto.Sku,
            ProductoCategoria  = l.Producto.Categoria,
            CantidadSistema    = l.CantidadSistema,
            CantidadFisica  = l.CantidadFisica,
            Diferencia      = l.Diferencia,
        }).ToList(),
    };
}
