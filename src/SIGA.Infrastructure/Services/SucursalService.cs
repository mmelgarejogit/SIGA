using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Stock;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class SucursalService(AppDbContext db) : ISucursalService
{
    public async Task<Result<IEnumerable<SucursalResponse>>> GetAllAsync(bool? isActive)
    {
        var query = db.Sucursales.AsQueryable();
        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        var items = await query.OrderBy(s => s.Nombre).ToListAsync();
        return Result<IEnumerable<SucursalResponse>>.Success(items.Select(ToResponse));
    }

    public async Task<Result<SucursalResponse>> GetByIdAsync(Guid id)
    {
        var s = await db.Sucursales.FindAsync(id);
        if (s is null)
            return Result<SucursalResponse>.Failure("Sucursal no encontrada.", ErrorType.NotFound);
        return Result<SucursalResponse>.Success(ToResponse(s));
    }

    public async Task<Result<SucursalResponse>> CreateAsync(CreateSucursalRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<SucursalResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.Codigo))
            return Result<SucursalResponse>.Failure("El código es obligatorio.", ErrorType.Validation);

        var codigo = request.Codigo.Trim().ToUpper();
        if (await db.Sucursales.AnyAsync(s => s.Codigo == codigo))
            return Result<SucursalResponse>.Failure("Ya existe una sucursal con ese código.", ErrorType.Conflict);

        var sucursal = new Sucursal
        {
            Nombre    = request.Nombre.Trim(),
            Codigo    = codigo,
            Direccion = request.Direccion?.Trim(),
            Telefono  = request.Telefono?.Trim(),
        };

        db.Sucursales.Add(sucursal);
        await db.SaveChangesAsync();
        return Result<SucursalResponse>.Success(ToResponse(sucursal));
    }

    public async Task<Result<SucursalResponse>> UpdateAsync(Guid id, UpdateSucursalRequest request)
    {
        var sucursal = await db.Sucursales.FindAsync(id);
        if (sucursal is null)
            return Result<SucursalResponse>.Failure("Sucursal no encontrada.", ErrorType.NotFound);

        var codigo = request.Codigo.Trim().ToUpper();
        if (await db.Sucursales.AnyAsync(s => s.Codigo == codigo && s.Id != id))
            return Result<SucursalResponse>.Failure("Ya existe una sucursal con ese código.", ErrorType.Conflict);

        sucursal.Nombre    = request.Nombre.Trim();
        sucursal.Codigo    = codigo;
        sucursal.Direccion = request.Direccion?.Trim();
        sucursal.Telefono  = request.Telefono?.Trim();
        sucursal.IsActive  = request.IsActive;

        await db.SaveChangesAsync();
        return Result<SucursalResponse>.Success(ToResponse(sucursal));
    }

    public async Task<Result<bool>> DeactivateAsync(Guid id)
    {
        var sucursal = await db.Sucursales.FindAsync(id);
        if (sucursal is null)
            return Result<bool>.Failure("Sucursal no encontrada.", ErrorType.NotFound);
        sucursal.IsActive = false;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static SucursalResponse ToResponse(Sucursal s) => new()
    {
        Id        = s.Id,
        Nombre    = s.Nombre,
        Codigo    = s.Codigo,
        Direccion = s.Direccion,
        Telefono  = s.Telefono,
        IsActive  = s.IsActive,
        CreatedAt = s.CreatedAt,
    };
}
