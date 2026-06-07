using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class TratamientoService(AppDbContext db) : ITratamientoService
{
    public async Task<Result<IEnumerable<TratamientoDto>>> GetAllAsync()
    {
        var items = await db.Tratamientos.OrderBy(t => t.Nombre).ToListAsync();
        return Result<IEnumerable<TratamientoDto>>.Success(items.Select(ToDto));
    }

    public async Task<Result<TratamientoDto>> CreateAsync(CreateTratamientoRequest request)
    {
        var nombre = request.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<TratamientoDto>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (await db.Tratamientos.AnyAsync(t => t.Nombre == nombre))
            return Result<TratamientoDto>.Failure("Ya existe un tratamiento con ese nombre.", ErrorType.Conflict);

        var item = new Tratamiento { Nombre = nombre, CreatedAt = DateTime.UtcNow };
        db.Tratamientos.Add(item);
        await db.SaveChangesAsync();
        return Result<TratamientoDto>.Success(ToDto(item));
    }

    public async Task<Result<TratamientoDto>> UpdateAsync(int id, UpdateTratamientoRequest request)
    {
        var item = await db.Tratamientos.FindAsync(id);
        if (item is null) return Result<TratamientoDto>.Failure("Tratamiento no encontrado.", ErrorType.NotFound);

        var nombre = request.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<TratamientoDto>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (await db.Tratamientos.AnyAsync(t => t.Nombre == nombre && t.Id != id))
            return Result<TratamientoDto>.Failure("Ya existe un tratamiento con ese nombre.", ErrorType.Conflict);

        item.Nombre   = nombre;
        item.IsActive = request.IsActive;
        await db.SaveChangesAsync();
        return Result<TratamientoDto>.Success(ToDto(item));
    }

    public async Task<Result<bool>> DeactivateAsync(int id)
    {
        var item = await db.Tratamientos.FindAsync(id);
        if (item is null) return Result<bool>.Failure("Tratamiento no encontrado.", ErrorType.NotFound);
        item.IsActive = false;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static TratamientoDto ToDto(Tratamiento t) => new()
    {
        Id        = t.Id,
        Nombre    = t.Nombre,
        IsActive  = t.IsActive,
        CreatedAt = t.CreatedAt,
    };
}
