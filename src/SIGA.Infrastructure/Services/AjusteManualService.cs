using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Stock;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class AjusteManualService(AppDbContext db) : IAjusteManualService
{
    public async Task<Result<PagedResult<AjusteManualResponse>>> GetAllAsync(
        int page, int pageSize, Guid? sucursalId, string? estado)
    {
        var query = db.AjustesManual
            .Include(a => a.Sucursal)
            .Include(a => a.TipoAjuste)
            .Include(a => a.ProductoVariante).ThenInclude(v => v.Producto)
            .Include(a => a.CreadoPor).ThenInclude(u => u.Person)
            .Include(a => a.AprobadoPor).ThenInclude(u => u!.Person)
            .AsQueryable();

        if (sucursalId.HasValue)
            query = query.Where(a => a.SucursalId == sucursalId.Value);

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<EstadoAjuste>(estado, out var e))
            query = query.Where(a => a.Estado == e);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.FechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<AjusteManualResponse>>.Success(new PagedResult<AjusteManualResponse>
        {
            Items      = items.Select(ToResponse),
            TotalCount = total,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
        });
    }

    public async Task<Result<AjusteManualResponse>> GetByIdAsync(Guid id)
    {
        var ajuste = await LoadById(id);
        if (ajuste is null)
            return Result<AjusteManualResponse>.Failure("Ajuste no encontrado.", ErrorType.NotFound);
        return Result<AjusteManualResponse>.Success(ToResponse(ajuste));
    }

    public async Task<Result<AjusteManualResponse>> CreateAsync(CreateAjusteManualRequest request, int usuarioId)
    {
        if (string.IsNullOrWhiteSpace(request.Observacion))
            return Result<AjusteManualResponse>.Failure("La observación es obligatoria.", ErrorType.Validation);

        if (request.Cantidad <= 0)
            return Result<AjusteManualResponse>.Failure("La cantidad debe ser mayor a cero.", ErrorType.Validation);

        var sucursal = await db.Sucursales.FindAsync(request.SucursalId);
        if (sucursal is null)
            return Result<AjusteManualResponse>.Failure("Sucursal no encontrada.", ErrorType.NotFound);

        if (InventarioFisicoService.TieneSesionActiva(db, request.SucursalId))
            return Result<AjusteManualResponse>.Failure("Hay un inventario físico en curso para esta sucursal. Los ajustes están bloqueados hasta que finalice.", ErrorType.Validation);

        var tipoAjuste = await db.TiposAjuste.FindAsync(request.TipoAjusteId);
        if (tipoAjuste is null || !tipoAjuste.Activo)
            return Result<AjusteManualResponse>.Failure("Tipo de ajuste no encontrado o inactivo.", ErrorType.NotFound);

        var variante = await db.ProductoVariantes.FindAsync(request.ProductoVarianteId);
        if (variante is null || !variante.IsActive)
            return Result<AjusteManualResponse>.Failure("Variante de producto no encontrada o inactiva.", ErrorType.NotFound);

        var ajuste = new AjusteManual
        {
            SucursalId         = request.SucursalId,
            TipoAjusteId       = request.TipoAjusteId,
            ProductoVarianteId = request.ProductoVarianteId,
            Cantidad           = request.Cantidad,
            Observacion        = request.Observacion.Trim(),
            CreadoPorId        = usuarioId,
        };

        db.AjustesManual.Add(ajuste);
        await db.SaveChangesAsync();

        return Result<AjusteManualResponse>.Success(ToResponse(await LoadById(ajuste.Id)!));
    }

    public async Task<Result<AjusteManualResponse>> ResolverAsync(
        Guid id, ResolverAjusteRequest request, int usuarioId)
    {
        var ajuste = await LoadById(id);
        if (ajuste is null)
            return Result<AjusteManualResponse>.Failure("Ajuste no encontrado.", ErrorType.NotFound);

        if (ajuste.Estado != EstadoAjuste.Pendiente)
            return Result<AjusteManualResponse>.Failure("Solo se pueden resolver ajustes en estado Pendiente.", ErrorType.Validation);

        if (request.Accion == "Aprobar")
        {
            ajuste.Estado = EstadoAjuste.Aprobado;

            // Determinar tipo de movimiento según impacto del tipo de ajuste y si hay ambigüedad
            var tipo = ajuste.TipoAjuste.Impacto switch
            {
                ImpactoAjuste.Positivo => TipoMovimiento.Ingreso,
                ImpactoAjuste.Negativo => TipoMovimiento.Egreso,
                _ => TipoMovimiento.Ingreso,
            };

            db.MovimientosInventario.Add(new MovimientoInventario
            {
                ProductoVarianteId = ajuste.ProductoVarianteId,
                SucursalId         = ajuste.SucursalId,
                Tipo               = tipo,
                Cantidad           = ajuste.Cantidad,
                UsuarioId          = usuarioId,
                OrigenTipo         = OrigenMovimiento.AjusteManual,
                ReferenciaId       = ajuste.Id,
                TipoAjusteId       = ajuste.TipoAjusteId,
            });
        }
        else if (request.Accion == "Rechazar")
        {
            ajuste.Estado = EstadoAjuste.Rechazado;
        }
        else
        {
            return Result<AjusteManualResponse>.Failure("Acción inválida. Use 'Aprobar' o 'Rechazar'.", ErrorType.Validation);
        }

        ajuste.AprobadoPorId          = usuarioId;
        ajuste.FechaResolucion         = DateTime.UtcNow;
        ajuste.ObservacionResolucion   = request.Observacion?.Trim();

        await db.SaveChangesAsync();
        return Result<AjusteManualResponse>.Success(ToResponse(await LoadById(ajuste.Id)!));
    }

    private Task<AjusteManual?> LoadById(Guid id) =>
        db.AjustesManual
            .Include(a => a.Sucursal)
            .Include(a => a.TipoAjuste)
            .Include(a => a.ProductoVariante).ThenInclude(v => v.Producto)
            .Include(a => a.CreadoPor).ThenInclude(u => u.Person)
            .Include(a => a.AprobadoPor!).ThenInclude(u => u.Person)
            .FirstOrDefaultAsync(a => a.Id == id);

    private static AjusteManualResponse ToResponse(AjusteManual a) => new()
    {
        Id                  = a.Id,
        SucursalId          = a.SucursalId,
        SucursalNombre      = a.Sucursal?.Nombre ?? "",
        TipoAjusteId        = a.TipoAjusteId,
        TipoAjusteNombre    = a.TipoAjuste?.Nombre ?? "",
        TipoAjusteImpacto   = a.TipoAjuste?.Impacto.ToString() ?? "",
        ProductoVarianteId  = a.ProductoVarianteId,
        ProductoNombre      = a.ProductoVariante?.Producto?.Nombre ?? "",
        VarianteSku         = a.ProductoVariante?.Sku,
        VarianteColor       = a.ProductoVariante?.Color,
        VarianteTalle       = a.ProductoVariante?.Talle,
        Cantidad            = a.Cantidad,
        Observacion         = a.Observacion,
        Estado              = a.Estado.ToString(),
        CreadoPorId         = a.CreadoPorId,
        CreadoPorNombre     = $"{a.CreadoPor?.Person?.FirstName} {a.CreadoPor?.Person?.LastName}".Trim(),
        AprobadoPorId       = a.AprobadoPorId,
        AprobadoPorNombre   = a.AprobadoPor == null ? null : $"{a.AprobadoPor.Person?.FirstName} {a.AprobadoPor.Person?.LastName}".Trim(),
        ObservacionResolucion = a.ObservacionResolucion,
        FechaCreacion       = a.FechaCreacion,
        FechaResolucion     = a.FechaResolucion,
    };
}
