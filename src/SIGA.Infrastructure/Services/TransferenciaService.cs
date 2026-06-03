using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Stock;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class TransferenciaService(AppDbContext db) : ITransferenciaService
{
    public async Task<Result<PagedResult<TransferenciaResponse>>> GetAllAsync(
        int page, int pageSize, Guid? sucursalId, string? estado)
    {
        var query = db.Transferencias
            .Include(t => t.SucursalOrigen)
            .Include(t => t.SucursalDestino)
            .Include(t => t.SolicitadoPor).ThenInclude(u => u.Person)
            .Include(t => t.AprobadoPor!).ThenInclude(u => u.Person)
            .Include(t => t.Lineas).ThenInclude(l => l.ProductoVariante).ThenInclude(v => v.Producto)
            .AsQueryable();

        if (sucursalId.HasValue)
            query = query.Where(t => t.SucursalOrigenId == sucursalId.Value
                                  || t.SucursalDestinoId == sucursalId.Value);

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoTransferencia>(estado, out var e))
            query = query.Where(t => t.Estado == e);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.FechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<TransferenciaResponse>>.Success(new PagedResult<TransferenciaResponse>
        {
            Items      = items.Select(ToResponse),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        });
    }

    public async Task<Result<TransferenciaResponse>> GetByIdAsync(Guid id)
    {
        var t = await LoadById(id);
        if (t is null)
            return Result<TransferenciaResponse>.Failure("Transferencia no encontrada.", ErrorType.NotFound);
        return Result<TransferenciaResponse>.Success(ToResponse(t));
    }

    public async Task<Result<TransferenciaResponse>> CreateAsync(
        CreateTransferenciaRequest request, int usuarioId)
    {
        if (!request.Lineas.Any())
            return Result<TransferenciaResponse>.Failure("La transferencia debe tener al menos una línea.", ErrorType.Validation);

        if (request.SucursalOrigenId == request.SucursalDestinoId)
            return Result<TransferenciaResponse>.Failure("La sucursal de origen y destino no pueden ser la misma.", ErrorType.Validation);

        var origen  = await db.Sucursales.FindAsync(request.SucursalOrigenId);
        var destino = await db.Sucursales.FindAsync(request.SucursalDestinoId);
        if (origen is null || !origen.IsActive)
            return Result<TransferenciaResponse>.Failure("Sucursal de origen no encontrada o inactiva.", ErrorType.NotFound);
        if (destino is null || !destino.IsActive)
            return Result<TransferenciaResponse>.Failure("Sucursal de destino no encontrada o inactiva.", ErrorType.NotFound);

        if (InventarioFisicoService.TieneSesionActiva(db, request.SucursalOrigenId))
            return Result<TransferenciaResponse>.Failure("Hay un inventario físico en curso en la sucursal de origen. Las transferencias están bloqueadas hasta que finalice.", ErrorType.Validation);

        if (InventarioFisicoService.TieneSesionActiva(db, request.SucursalDestinoId))
            return Result<TransferenciaResponse>.Failure("Hay un inventario físico en curso en la sucursal de destino. Las transferencias están bloqueadas hasta que finalice.", ErrorType.Validation);

        foreach (var linea in request.Lineas)
        {
            if (linea.Cantidad <= 0)
                return Result<TransferenciaResponse>.Failure("Todas las cantidades deben ser mayores a cero.", ErrorType.Validation);

            var variante = await db.ProductoVariantes.FindAsync(linea.ProductoVarianteId);
            if (variante is null || !variante.IsActive)
                return Result<TransferenciaResponse>.Failure($"Variante {linea.ProductoVarianteId} no encontrada o inactiva.", ErrorType.NotFound);
        }

        var transferencia = new Transferencia
        {
            SucursalOrigenId  = request.SucursalOrigenId,
            SucursalDestinoId = request.SucursalDestinoId,
            Observacion       = request.Observacion?.Trim(),
            SolicitadoPorId   = usuarioId,
            Lineas            = request.Lineas.Select(l => new TransferenciaLinea
            {
                ProductoVarianteId = l.ProductoVarianteId,
                Cantidad           = l.Cantidad,
            }).ToList(),
        };

        db.Transferencias.Add(transferencia);
        await db.SaveChangesAsync();

        return Result<TransferenciaResponse>.Success(ToResponse(await LoadById(transferencia.Id)!));
    }

    public async Task<Result<TransferenciaResponse>> ResolverAsync(
        Guid id, ResolverTransferenciaRequest request, int usuarioId)
    {
        var transferencia = await LoadById(id);
        if (transferencia is null)
            return Result<TransferenciaResponse>.Failure("Transferencia no encontrada.", ErrorType.NotFound);

        if (transferencia.Estado != EstadoTransferencia.Solicitada)
            return Result<TransferenciaResponse>.Failure("Solo se pueden resolver transferencias en estado Solicitada.", ErrorType.Validation);

        if (request.Accion == "Aprobar")
        {
            // Validar stock disponible en origen al momento de aprobación
            foreach (var linea in transferencia.Lineas)
            {
                var stockActual = await db.MovimientosInventario
                    .Where(m => m.ProductoVarianteId == linea.ProductoVarianteId
                             && m.SucursalId == transferencia.SucursalOrigenId)
                    .SumAsync(m => m.Tipo == TipoMovimiento.Ingreso ? m.Cantidad : -m.Cantidad);

                if (stockActual < linea.Cantidad)
                {
                    var variante = linea.ProductoVariante;
                    var nombre   = variante?.Producto?.Nombre ?? "Variante";
                    return Result<TransferenciaResponse>.Failure(
                        $"Stock insuficiente para \"{nombre}\" (disponible: {stockActual}, solicitado: {linea.Cantidad}).",
                        ErrorType.Validation);
                }
            }

            transferencia.Estado = EstadoTransferencia.Aprobada;

            // Generar movimientos atómicos: EGRESO en origen, INGRESO en destino
            var fecha = DateTime.UtcNow;
            foreach (var linea in transferencia.Lineas)
            {
                db.MovimientosInventario.Add(new MovimientoInventario
                {
                    ProductoVarianteId = linea.ProductoVarianteId,
                    SucursalId         = transferencia.SucursalOrigenId,
                    Tipo               = TipoMovimiento.Egreso,
                    Cantidad           = linea.Cantidad,
                    UsuarioId          = usuarioId,
                    OrigenTipo         = OrigenMovimiento.Transferencia,
                    ReferenciaId       = transferencia.Id,
                    Fecha              = fecha,
                });

                db.MovimientosInventario.Add(new MovimientoInventario
                {
                    ProductoVarianteId = linea.ProductoVarianteId,
                    SucursalId         = transferencia.SucursalDestinoId,
                    Tipo               = TipoMovimiento.Ingreso,
                    Cantidad           = linea.Cantidad,
                    UsuarioId          = usuarioId,
                    OrigenTipo         = OrigenMovimiento.Transferencia,
                    ReferenciaId       = transferencia.Id,
                    Fecha              = fecha,
                });
            }
        }
        else if (request.Accion == "Rechazar")
        {
            if (string.IsNullOrWhiteSpace(request.MotivoRechazo))
                return Result<TransferenciaResponse>.Failure("El motivo de rechazo es obligatorio.", ErrorType.Validation);

            transferencia.Estado         = EstadoTransferencia.Rechazada;
            transferencia.MotivoRechazo  = request.MotivoRechazo.Trim();
        }
        else
        {
            return Result<TransferenciaResponse>.Failure("Acción inválida. Use 'Aprobar' o 'Rechazar'.", ErrorType.Validation);
        }

        transferencia.AprobadoPorId   = usuarioId;
        transferencia.FechaResolucion = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Result<TransferenciaResponse>.Success(ToResponse(await LoadById(transferencia.Id)!));
    }

    private Task<Transferencia?> LoadById(Guid id) =>
        db.Transferencias
            .Include(t => t.SucursalOrigen)
            .Include(t => t.SucursalDestino)
            .Include(t => t.SolicitadoPor).ThenInclude(u => u.Person)
            .Include(t => t.AprobadoPor!).ThenInclude(u => u.Person)
            .Include(t => t.Lineas).ThenInclude(l => l.ProductoVariante).ThenInclude(v => v.Producto)
            .FirstOrDefaultAsync(t => t.Id == id);

    private static TransferenciaResponse ToResponse(Transferencia t) => new()
    {
        Id                    = t.Id,
        SucursalOrigenId      = t.SucursalOrigenId,
        SucursalOrigenNombre  = t.SucursalOrigen?.Nombre ?? "",
        SucursalDestinoId     = t.SucursalDestinoId,
        SucursalDestinoNombre = t.SucursalDestino?.Nombre ?? "",
        Estado                = t.Estado.ToString(),
        SolicitadoPorId       = t.SolicitadoPorId,
        SolicitadoPorNombre   = $"{t.SolicitadoPor?.Person?.FirstName} {t.SolicitadoPor?.Person?.LastName}".Trim(),
        AprobadoPorId         = t.AprobadoPorId,
        AprobadoPorNombre     = t.AprobadoPor == null ? null : $"{t.AprobadoPor.Person?.FirstName} {t.AprobadoPor.Person?.LastName}".Trim(),
        Observacion           = t.Observacion,
        MotivoRechazo         = t.MotivoRechazo,
        FechaCreacion         = t.FechaCreacion,
        FechaResolucion       = t.FechaResolucion,
        Lineas                = t.Lineas.Select(l => new TransferenciaLineaResponse
        {
            Id                 = l.Id,
            ProductoVarianteId = l.ProductoVarianteId,
            ProductoNombre     = l.ProductoVariante?.Producto?.Nombre ?? "",
            Sku                = l.ProductoVariante?.Sku,
            Color              = l.ProductoVariante?.Color,
            Talle              = l.ProductoVariante?.Talle,
            Cantidad           = l.Cantidad,
        }).ToList(),
    };
}
