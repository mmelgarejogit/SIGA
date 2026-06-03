using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Stock;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class TipoAjusteService(AppDbContext db) : ITipoAjusteService
{
    public async Task<Result<IEnumerable<TipoAjusteResponse>>> GetAllAsync(string? impacto, bool? activo)
    {
        var query = db.TiposAjuste.AsQueryable();

        if (!string.IsNullOrWhiteSpace(impacto) && Enum.TryParse<ImpactoAjuste>(impacto, out var imp))
            query = query.Where(t => t.Impacto == imp || t.Impacto == ImpactoAjuste.Ambos);

        if (activo.HasValue)
            query = query.Where(t => t.Activo == activo.Value);

        var items = await query.OrderBy(t => t.Nombre).ToListAsync();
        return Result<IEnumerable<TipoAjusteResponse>>.Success(items.Select(ToResponse));
    }

    public async Task<Result<TipoAjusteResponse>> GetByIdAsync(Guid id)
    {
        var t = await db.TiposAjuste.FindAsync(id);
        if (t is null)
            return Result<TipoAjusteResponse>.Failure("Tipo de ajuste no encontrado.", ErrorType.NotFound);
        return Result<TipoAjusteResponse>.Success(ToResponse(t));
    }

    public async Task<Result<TipoAjusteResponse>> CreateAsync(CreateTipoAjusteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<TipoAjusteResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (!Enum.TryParse<ImpactoAjuste>(request.Impacto, out var impacto))
            return Result<TipoAjusteResponse>.Failure("Impacto inválido. Use: Positivo, Negativo o Ambos.", ErrorType.Validation);

        var nombre = request.Nombre.Trim();
        if (await db.TiposAjuste.AnyAsync(t => t.Nombre == nombre))
            return Result<TipoAjusteResponse>.Failure("Ya existe un tipo de ajuste con ese nombre.", ErrorType.Conflict);

        var tipo = new TipoAjuste { Nombre = nombre, Impacto = impacto };
        db.TiposAjuste.Add(tipo);
        await db.SaveChangesAsync();
        return Result<TipoAjusteResponse>.Success(ToResponse(tipo));
    }

    public async Task<Result<TipoAjusteResponse>> UpdateAsync(Guid id, UpdateTipoAjusteRequest request)
    {
        var tipo = await db.TiposAjuste.FindAsync(id);
        if (tipo is null)
            return Result<TipoAjusteResponse>.Failure("Tipo de ajuste no encontrado.", ErrorType.NotFound);

        if (!Enum.TryParse<ImpactoAjuste>(request.Impacto, out var impacto))
            return Result<TipoAjusteResponse>.Failure("Impacto inválido.", ErrorType.Validation);

        var nombre = request.Nombre.Trim();
        if (await db.TiposAjuste.AnyAsync(t => t.Nombre == nombre && t.Id != id))
            return Result<TipoAjusteResponse>.Failure("Ya existe un tipo de ajuste con ese nombre.", ErrorType.Conflict);

        tipo.Nombre  = nombre;
        tipo.Impacto = impacto;
        tipo.Activo  = request.Activo;
        await db.SaveChangesAsync();
        return Result<TipoAjusteResponse>.Success(ToResponse(tipo));
    }

    public async Task<Result<bool>> DeactivateAsync(Guid id)
    {
        var tipo = await db.TiposAjuste.FindAsync(id);
        if (tipo is null)
            return Result<bool>.Failure("Tipo de ajuste no encontrado.", ErrorType.NotFound);
        tipo.Activo = false;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static TipoAjusteResponse ToResponse(TipoAjuste t) => new()
    {
        Id      = t.Id,
        Nombre  = t.Nombre,
        Impacto = t.Impacto.ToString(),
        Activo  = t.Activo,
    };
}
