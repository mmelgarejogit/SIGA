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

        DateOnly? vigencia = null;
        if (!string.IsNullOrWhiteSpace(request.VigenciaTimbrado))
        {
            if (!DateOnly.TryParse(request.VigenciaTimbrado, out var v))
                return Result<ProveedorResponse>.Failure("Fecha de vigencia del timbrado inválida.", ErrorType.Validation);
            if (v <= DateOnly.FromDateTime(DateTime.UtcNow))
                return Result<ProveedorResponse>.Failure("La vigencia del timbrado debe ser una fecha futura.", ErrorType.Validation);
            vigencia = v;
        }

        var proveedor = new Proveedor
        {
            Nombre            = request.Nombre.Trim(),
            Contacto          = request.Contacto?.Trim(),
            Email             = request.Email?.Trim(),
            Telefono          = request.Telefono?.Trim(),
            Ruc               = request.Ruc.Trim(),
            Timbrado          = request.Timbrado.Trim(),
            VigenciaTimbrado  = vigencia,
            Establecimiento   = request.Establecimiento?.Trim(),
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

        DateOnly? vigencia = null;
        if (!string.IsNullOrWhiteSpace(request.VigenciaTimbrado))
        {
            if (!DateOnly.TryParse(request.VigenciaTimbrado, out var v))
                return Result<ProveedorResponse>.Failure("Fecha de vigencia del timbrado inválida.", ErrorType.Validation);
            if (v <= DateOnly.FromDateTime(DateTime.UtcNow))
                return Result<ProveedorResponse>.Failure("La vigencia del timbrado debe ser una fecha futura.", ErrorType.Validation);
            vigencia = v;
        }

        proveedor.Nombre           = request.Nombre.Trim();
        proveedor.Contacto         = request.Contacto?.Trim();
        proveedor.Email            = request.Email?.Trim();
        proveedor.Telefono         = request.Telefono?.Trim();
        proveedor.Ruc              = request.Ruc.Trim();
        proveedor.Timbrado         = request.Timbrado.Trim();
        proveedor.VigenciaTimbrado = vigencia;
        proveedor.Establecimiento  = request.Establecimiento?.Trim();
        proveedor.UpdatedAt        = DateTime.UtcNow;

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
        Id               = p.Id,
        Nombre           = p.Nombre,
        Contacto         = p.Contacto,
        Email            = p.Email,
        Telefono         = p.Telefono,
        Ruc              = p.Ruc,
        Timbrado         = p.Timbrado,
        VigenciaTimbrado = p.VigenciaTimbrado?.ToString("yyyy-MM-dd"),
        Establecimiento  = p.Establecimiento,
        IsActive         = p.IsActive,
        CreatedAt        = p.CreatedAt,
    };
}
