using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class ProveedorService(AppDbContext db) : IProveedorService
{
    public async Task<Result<IEnumerable<ProveedorResponse>>> GetAllAsync(string? search)
    {
        var query = db.Proveedores.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            query = query.Where(p => p.Nombre.ToLower().Contains(q));
        }

        var proveedores = await query.OrderBy(p => p.Nombre).ToListAsync();
        return Result<IEnumerable<ProveedorResponse>>.Success(proveedores.Select(ToResponse));
    }

    public async Task<Result<ProveedorResponse>> CreateAsync(CreateProveedorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<ProveedorResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        var proveedor = new Proveedor
        {
            Nombre   = request.Nombre.Trim(),
            Contacto = request.Contacto?.Trim(),
            Email    = request.Email?.Trim(),
            Telefono = request.Telefono?.Trim(),
        };

        db.Proveedores.Add(proveedor);
        await db.SaveChangesAsync();
        return Result<ProveedorResponse>.Success(ToResponse(proveedor));
    }

    public async Task<Result<ProveedorResponse>> UpdateAsync(int id, CreateProveedorRequest request)
    {
        var proveedor = await db.Proveedores.FindAsync(id);
        if (proveedor is null)
            return Result<ProveedorResponse>.Failure("Proveedor no encontrado.", ErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<ProveedorResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        proveedor.Nombre    = request.Nombre.Trim();
        proveedor.Contacto  = request.Contacto?.Trim();
        proveedor.Email     = request.Email?.Trim();
        proveedor.Telefono  = request.Telefono?.Trim();
        proveedor.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Result<ProveedorResponse>.Success(ToResponse(proveedor));
    }

    public async Task<Result<bool>> DeactivateAsync(int id)
    {
        var proveedor = await db.Proveedores.FindAsync(id);
        if (proveedor is null)
            return Result<bool>.Failure("Proveedor no encontrado.", ErrorType.NotFound);

        proveedor.IsActive  = false;
        proveedor.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static ProveedorResponse ToResponse(Proveedor p) => new()
    {
        Id        = p.Id,
        Nombre    = p.Nombre,
        Contacto  = p.Contacto,
        Email     = p.Email,
        Telefono  = p.Telefono,
        IsActive  = p.IsActive,
        CreatedAt = p.CreatedAt,
    };
}
