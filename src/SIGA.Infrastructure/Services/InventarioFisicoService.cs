using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Stock;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class InventarioFisicoService(AppDbContext db) : IInventarioFisicoService
{
    public async Task<Result<PagedResult<InventarioFisicoResponse>>> GetAllAsync(
        int page, int pageSize, Guid? sucursalId, string? estado)
    {
        var query = db.InventariosFisicos
            .Include(i => i.Sucursal)
            .Include(i => i.IniciadoPor).ThenInclude(u => u.Person)
            .Include(i => i.Lineas)
            .AsQueryable();

        if (sucursalId.HasValue)
            query = query.Where(i => i.SucursalId == sucursalId.Value);

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoInventario>(estado, out var e))
            query = query.Where(i => i.Estado == e);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<InventarioFisicoResponse>>.Success(new PagedResult<InventarioFisicoResponse>
        {
            Items      = items.Select(i => ToResponse(i, includeSnapshot: false)),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        });
    }

    public async Task<Result<InventarioFisicoResponse>> GetByIdAsync(Guid id, bool includeSnapshot)
    {
        var inv = await LoadById(id);
        if (inv is null)
            return Result<InventarioFisicoResponse>.Failure("Inventario físico no encontrado.", ErrorType.NotFound);
        return Result<InventarioFisicoResponse>.Success(ToResponse(inv, includeSnapshot));
    }

    public async Task<Result<InventarioFisicoResponse>> CreateAsync(
        CreateInventarioFisicoRequest request, int adminUserId)
    {
        var sucursal = await db.Sucursales.FindAsync(request.SucursalId);
        if (sucursal is null || !sucursal.IsActive)
            return Result<InventarioFisicoResponse>.Failure("Sucursal no encontrada o inactiva.", ErrorType.NotFound);

        if (!Enum.TryParse<AlcanceInventario>(request.Alcance, out var alcance))
            return Result<InventarioFisicoResponse>.Failure("Alcance inválido. Use Total o Parcial.", ErrorType.Validation);

        if (alcance == AlcanceInventario.Parcial && !request.FiltroCategoriaId.HasValue)
            return Result<InventarioFisicoResponse>.Failure("Para alcance Parcial se requiere una categoría.", ErrorType.Validation);

        var inv = new InventarioFisico
        {
            SucursalId       = request.SucursalId,
            Alcance          = alcance,
            FiltroCategoriaId = alcance == AlcanceInventario.Parcial ? request.FiltroCategoriaId : null,
            IniciadoPorId    = adminUserId,
            Observacion      = request.Observacion?.Trim(),
        };

        db.InventariosFisicos.Add(inv);
        await db.SaveChangesAsync();

        return Result<InventarioFisicoResponse>.Success(ToResponse(await LoadById(inv.Id)!, false));
    }

    public async Task<Result<InventarioFisicoResponse>> IniciarConteoAsync(Guid id, int adminUserId)
    {
        var inv = await LoadById(id);
        if (inv is null)
            return Result<InventarioFisicoResponse>.Failure("Inventario físico no encontrado.", ErrorType.NotFound);

        if (inv.Estado != EstadoInventario.Borrador)
            return Result<InventarioFisicoResponse>.Failure("Solo se puede iniciar el conteo desde estado Borrador.", ErrorType.Validation);

        // Obtener variantes en scope
        var variantesQuery = db.ProductoVariantes
            .Include(v => v.Producto)
            .Where(v => v.IsActive);

        if (inv.Alcance == AlcanceInventario.Parcial && inv.FiltroCategoriaId.HasValue)
            variantesQuery = variantesQuery.Where(v => v.Producto.CategoriaProductoId == inv.FiltroCategoriaId);

        var variantes = await variantesQuery.ToListAsync();

        if (!variantes.Any())
            return Result<InventarioFisicoResponse>.Failure("No hay variantes activas en el alcance seleccionado.", ErrorType.Validation);

        // Snapshot de stock actual para cada variante en esta sucursal
        var varianteIds = variantes.Select(v => v.Id).ToList();
        var stockMap = await db.MovimientosInventario
            .Where(m => varianteIds.Contains(m.ProductoVarianteId) && m.SucursalId == inv.SucursalId)
            .GroupBy(m => m.ProductoVarianteId)
            .Select(g => new
            {
                VarianteId = g.Key,
                Stock = g.Sum(m => m.Tipo == TipoMovimiento.Ingreso ? m.Cantidad : -m.Cantidad),
            })
            .ToDictionaryAsync(x => x.VarianteId, x => x.Stock);

        var lineas = variantes.Select(v => new InventarioFisicoLinea
        {
            InventarioFisicoId = inv.Id,
            ProductoVarianteId = v.Id,
            CantidadSistema    = stockMap.GetValueOrDefault(v.Id, 0),
        }).ToList();

        db.InventarioFisicoLineas.AddRange(lineas);

        inv.Estado           = EstadoInventario.EnConteo;
        inv.FechaInicioConteo = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Result<InventarioFisicoResponse>.Success(ToResponse(await LoadById(id)!, true));
    }

    public async Task<Result<InventarioFisicoResponse>> GuardarConteosAsync(
        Guid id, GuardarConteosRequest request, int encargadoUserId)
    {
        var inv = await LoadById(id);
        if (inv is null)
            return Result<InventarioFisicoResponse>.Failure("Inventario físico no encontrado.", ErrorType.NotFound);

        if (inv.Estado != EstadoInventario.EnConteo)
            return Result<InventarioFisicoResponse>.Failure("Solo se pueden guardar conteos cuando el inventario está EN_CONTEO.", ErrorType.Validation);

        var lineaMap = inv.Lineas.ToDictionary(l => l.Id);

        foreach (var item in request.Lineas)
        {
            if (!lineaMap.TryGetValue(item.LineaId, out var linea)) continue;
            if (item.CantidadContada.HasValue && item.CantidadContada < 0)
                return Result<InventarioFisicoResponse>.Failure("La cantidad contada no puede ser negativa.", ErrorType.Validation);
            linea.CantidadContada = item.CantidadContada;
        }

        if (!inv.EjecutadoPorId.HasValue)
            inv.EjecutadoPorId = encargadoUserId;

        await db.SaveChangesAsync();
        return Result<InventarioFisicoResponse>.Success(ToResponse(await LoadById(id)!, false));
    }

    public async Task<Result<InventarioFisicoResponse>> CerrarAsync(Guid id, int encargadoUserId)
    {
        var inv = await LoadById(id);
        if (inv is null)
            return Result<InventarioFisicoResponse>.Failure("Inventario físico no encontrado.", ErrorType.NotFound);

        if (inv.Estado != EstadoInventario.EnConteo)
            return Result<InventarioFisicoResponse>.Failure("Solo se puede cerrar un inventario en estado EN_CONTEO.", ErrorType.Validation);

        if (inv.Lineas.Any(l => !l.CantidadContada.HasValue))
            return Result<InventarioFisicoResponse>.Failure("Hay líneas sin cantidad contada. Completá todas las líneas antes de cerrar.", ErrorType.Validation);

        foreach (var linea in inv.Lineas)
            linea.Diferencia = linea.CantidadContada!.Value - linea.CantidadSistema;

        inv.Estado         = EstadoInventario.Cerrado;
        inv.EjecutadoPorId = encargadoUserId;

        await db.SaveChangesAsync();
        return Result<InventarioFisicoResponse>.Success(ToResponse(await LoadById(id)!, true));
    }

    public async Task<Result<InventarioFisicoResponse>> AprobarAsync(Guid id, int adminUserId)
    {
        var inv = await LoadById(id);
        if (inv is null)
            return Result<InventarioFisicoResponse>.Failure("Inventario físico no encontrado.", ErrorType.NotFound);

        if (inv.Estado != EstadoInventario.Cerrado)
            return Result<InventarioFisicoResponse>.Failure("Solo se puede aprobar un inventario en estado CERRADO.", ErrorType.Validation);

        var fecha = DateTime.UtcNow;
        var movimientos = inv.Lineas
            .Where(l => l.Diferencia.HasValue && l.Diferencia != 0)
            .Select(l => new MovimientoInventario
            {
                ProductoVarianteId = l.ProductoVarianteId,
                SucursalId         = inv.SucursalId,
                Tipo               = l.Diferencia > 0 ? TipoMovimiento.Ingreso : TipoMovimiento.Egreso,
                Cantidad           = Math.Abs(l.Diferencia!.Value),
                UsuarioId          = adminUserId,
                OrigenTipo         = OrigenMovimiento.CorreccionConteo,
                ReferenciaId       = inv.Id,
                Fecha              = fecha,
            })
            .ToList();

        db.MovimientosInventario.AddRange(movimientos);

        inv.Estado          = EstadoInventario.Aprobado;
        inv.AprobadoPorId   = adminUserId;
        inv.FechaResolucion = fecha;

        await db.SaveChangesAsync();
        return Result<InventarioFisicoResponse>.Success(ToResponse(await LoadById(id)!, true));
    }

    public async Task<Result<InventarioFisicoResponse>> CancelarAsync(Guid id, int userId)
    {
        var inv = await LoadById(id);
        if (inv is null)
            return Result<InventarioFisicoResponse>.Failure("Inventario físico no encontrado.", ErrorType.NotFound);

        if (inv.Estado is EstadoInventario.Aprobado or EstadoInventario.Cancelado)
            return Result<InventarioFisicoResponse>.Failure("No se puede cancelar un inventario ya resuelto.", ErrorType.Validation);

        inv.Estado          = EstadoInventario.Cancelado;
        inv.AprobadoPorId   = userId;
        inv.FechaResolucion = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Result<InventarioFisicoResponse>.Success(ToResponse(await LoadById(id)!, false));
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    internal static bool TieneSesionActiva(AppDbContext db, Guid sucursalId)
        => db.InventariosFisicos.Any(i => i.SucursalId == sucursalId && i.Estado == EstadoInventario.EnConteo);

    private Task<InventarioFisico?> LoadById(Guid id) =>
        db.InventariosFisicos
            .Include(i => i.Sucursal)
            .Include(i => i.FiltroCategoria)
            .Include(i => i.IniciadoPor).ThenInclude(u => u.Person)
            .Include(i => i.EjecutadoPor!).ThenInclude(u => u.Person)
            .Include(i => i.AprobadoPor!).ThenInclude(u => u.Person)
            .Include(i => i.Lineas).ThenInclude(l => l.ProductoVariante).ThenInclude(v => v.Producto)
            .FirstOrDefaultAsync(i => i.Id == id);

    private static InventarioFisicoResponse ToResponse(InventarioFisico i, bool includeSnapshot) => new()
    {
        Id                    = i.Id,
        SucursalId            = i.SucursalId,
        SucursalNombre        = i.Sucursal?.Nombre ?? "",
        Estado                = i.Estado.ToString(),
        Alcance               = i.Alcance.ToString(),
        FiltroCategoriaId     = i.FiltroCategoriaId,
        FiltroCategoriaNombre = i.FiltroCategoria?.Nombre,
        FechaInicioConteo     = i.FechaInicioConteo,
        IniciadoPorId         = i.IniciadoPorId,
        IniciadoPorNombre     = $"{i.IniciadoPor?.Person?.FirstName} {i.IniciadoPor?.Person?.LastName}".Trim(),
        EjecutadoPorId        = i.EjecutadoPorId,
        EjecutadoPorNombre    = i.EjecutadoPor == null ? null : $"{i.EjecutadoPor.Person?.FirstName} {i.EjecutadoPor.Person?.LastName}".Trim(),
        AprobadoPorId         = i.AprobadoPorId,
        AprobadoPorNombre     = i.AprobadoPor == null ? null : $"{i.AprobadoPor.Person?.FirstName} {i.AprobadoPor.Person?.LastName}".Trim(),
        Observacion           = i.Observacion,
        CreatedAt             = i.CreatedAt,
        FechaResolucion       = i.FechaResolucion,
        TotalLineas           = i.Lineas.Count,
        LineasContadas        = i.Lineas.Count(l => l.CantidadContada.HasValue),
        LineasConDiferencia   = i.Lineas.Count(l => l.Diferencia.HasValue && l.Diferencia != 0),
        Lineas                = i.Lineas.Select(l => new InventarioFisicoLineaResponse
        {
            Id                = l.Id,
            ProductoVarianteId = l.ProductoVarianteId,
            ProductoNombre    = l.ProductoVariante?.Producto?.Nombre ?? "",
            VarianteSku       = l.ProductoVariante?.Sku,
            VarianteColor     = l.ProductoVariante?.Color,
            VarianteTalle     = l.ProductoVariante?.Talle,
            CantidadSistema   = includeSnapshot ? l.CantidadSistema : null,
            CantidadContada   = l.CantidadContada,
            Diferencia        = includeSnapshot ? l.Diferencia : null,
        }).ToList(),
    };
}
