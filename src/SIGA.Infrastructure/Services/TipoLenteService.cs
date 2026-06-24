using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class TipoLenteService(AppDbContext db) : ITipoLenteService
{
    public async Task<Result<IEnumerable<TipoLenteDto>>> GetAllAsync()
    {
        var items = await db.TiposLente.OrderBy(t => t.Nombre).ToListAsync();
        return Result<IEnumerable<TipoLenteDto>>.Success(items.Select(ToDto));
    }

    public async Task<Result<TipoLenteDto>> CreateAsync(CreateTipoLenteRequest request)
    {
        var nombre = request.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<TipoLenteDto>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (await db.TiposLente.AnyAsync(t => t.Nombre == nombre))
            return Result<TipoLenteDto>.Failure("Ya existe un tipo de lente con ese nombre.", ErrorType.Conflict);

        var item = new TipoLente { Nombre = nombre, PrecioBase = request.PrecioBase, CreatedAt = DateTime.UtcNow };
        db.TiposLente.Add(item);
        await db.SaveChangesAsync();
        return Result<TipoLenteDto>.Success(ToDto(item));
    }

    public async Task<Result<TipoLenteDto>> UpdateAsync(int id, UpdateTipoLenteRequest request)
    {
        var item = await db.TiposLente.FindAsync(id);
        if (item is null) return Result<TipoLenteDto>.Failure("Tipo de lente no encontrado.", ErrorType.NotFound);

        var nombre = request.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<TipoLenteDto>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (await db.TiposLente.AnyAsync(t => t.Nombre == nombre && t.Id != id))
            return Result<TipoLenteDto>.Failure("Ya existe un tipo de lente con ese nombre.", ErrorType.Conflict);

        item.Nombre     = nombre;
        item.PrecioBase = request.PrecioBase;
        item.IsActive   = request.IsActive;
        await db.SaveChangesAsync();
        return Result<TipoLenteDto>.Success(ToDto(item));
    }

    public async Task<Result<bool>> DeactivateAsync(int id)
    {
        var item = await db.TiposLente.FindAsync(id);
        if (item is null) return Result<bool>.Failure("Tipo de lente no encontrado.", ErrorType.NotFound);
        item.IsActive = false;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var item = await db.TiposLente.FindAsync(id);
        if (item is null) return Result<bool>.Failure("Tipo de lente no encontrado.", ErrorType.NotFound);

        if (await db.TrabajosPedido.AnyAsync(t => t.TipoLenteId == id))
            return Result<bool>.Failure(
                "No se puede eliminar: el tipo de lente está en uso en trabajos a pedido. Desactivalo en su lugar.",
                ErrorType.Conflict);

        db.TiposLente.Remove(item);
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static TipoLenteDto ToDto(TipoLente t) => new()
    {
        Id         = t.Id,
        Nombre     = t.Nombre,
        PrecioBase = t.PrecioBase,
        IsActive   = t.IsActive,
        CreatedAt  = t.CreatedAt,
    };
}
