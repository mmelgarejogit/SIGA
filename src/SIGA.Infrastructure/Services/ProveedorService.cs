using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class ProveedorService(AppDbContext db) : IProveedorService
{
    public async Task<Result<PagedResult<ProveedorResponse>>> GetAllAsync(int page, int pageSize, string? search, bool? isActive)
    {
        page     = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = db.Proveedores.Include(p => p.Contactos).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = $"%{search.Trim()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Nombre, q) ||
                (p.RazonSocial != null && EF.Functions.ILike(p.RazonSocial, q)) ||
                EF.Functions.ILike(p.Ruc, q));
        }

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var totalCount  = await query.CountAsync();
        var totalActive = await db.Proveedores.CountAsync(p => p.IsActive);
        var totalPages  = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderBy(p => p.Nombre)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Result<PagedResult<ProveedorResponse>>.Success(new PagedResult<ProveedorResponse>
        {
            Items       = items.Select(ToResponse),
            TotalCount  = totalCount,
            TotalActive = totalActive,
            Page        = page,
            PageSize    = pageSize,
            TotalPages  = Math.Max(1, totalPages),
        });
    }

    public async Task<Result<ProveedorResponse>> CreateAsync(CreateProveedorRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<ProveedorResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (!request.Contactos.Any(c => !string.IsNullOrWhiteSpace(c.Nombre)))
            return Result<ProveedorResponse>.Failure("Debe registrar al menos un contacto.", ErrorType.Validation);

        var proveedor = new Proveedor
        {
            Nombre     = request.Nombre.Trim(),
            RazonSocial = request.RazonSocial?.Trim(),
            Ruc        = request.Ruc.Trim(),
            Direccion  = request.Direccion?.Trim(),
            Ciudad     = request.Ciudad?.Trim(),
            SitioWeb   = request.SitioWeb?.Trim(),
            Facebook   = request.Facebook?.Trim(),
            Instagram  = request.Instagram?.Trim(),
            WhatsApp      = request.WhatsApp?.Trim(),
            EsLaboratorio = request.EsLaboratorio,
        };

        foreach (var c in request.Contactos.Where(c => !string.IsNullOrWhiteSpace(c.Nombre)))
        {
            proveedor.Contactos.Add(new ProveedorContacto
            {
                Nombre   = c.Nombre.Trim(),
                Cargo    = c.Cargo?.Trim(),
                Telefono = c.Telefono?.Trim(),
                Email    = c.Email?.Trim(),
            });
        }

        db.Proveedores.Add(proveedor);
        await db.SaveChangesAsync();
        return Result<ProveedorResponse>.Success(ToResponse(proveedor));
    }

    public async Task<Result<ProveedorResponse>> UpdateAsync(int id, CreateProveedorRequest request)
    {
        var proveedor = await db.Proveedores.Include(p => p.Contactos).FirstOrDefaultAsync(p => p.Id == id);
        if (proveedor is null)
            return Result<ProveedorResponse>.Failure("Proveedor no encontrado.", ErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<ProveedorResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (!request.Contactos.Any(c => !string.IsNullOrWhiteSpace(c.Nombre)))
            return Result<ProveedorResponse>.Failure("Debe registrar al menos un contacto.", ErrorType.Validation);

        proveedor.Nombre      = request.Nombre.Trim();
        proveedor.RazonSocial = request.RazonSocial?.Trim();
        proveedor.Ruc         = request.Ruc.Trim();
        proveedor.Direccion   = request.Direccion?.Trim();
        proveedor.Ciudad      = request.Ciudad?.Trim();
        proveedor.SitioWeb    = request.SitioWeb?.Trim();
        proveedor.Facebook    = request.Facebook?.Trim();
        proveedor.Instagram   = request.Instagram?.Trim();
        proveedor.WhatsApp      = request.WhatsApp?.Trim();
        proveedor.EsLaboratorio = request.EsLaboratorio;
        proveedor.UpdatedAt     = DateTime.UtcNow;

        // Reemplaza todos los contactos
        db.Set<ProveedorContacto>().RemoveRange(proveedor.Contactos);
        proveedor.Contactos.Clear();

        foreach (var c in request.Contactos.Where(c => !string.IsNullOrWhiteSpace(c.Nombre)))
        {
            proveedor.Contactos.Add(new ProveedorContacto
            {
                Nombre   = c.Nombre.Trim(),
                Cargo    = c.Cargo?.Trim(),
                Telefono = c.Telefono?.Trim(),
                Email    = c.Email?.Trim(),
            });
        }

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

    public async Task<Result<List<ProveedorResponse>>> GetLaboratoriosAsync()
    {
        var labs = await db.Proveedores
            .Include(p => p.Contactos)
            .Where(p => p.EsLaboratorio && p.IsActive)
            .OrderBy(p => p.Nombre)
            .ToListAsync();
        return Result<List<ProveedorResponse>>.Success(labs.Select(ToResponse).ToList());
    }

    private static ProveedorResponse ToResponse(Proveedor p) => new()
    {
        Id          = p.Id,
        Nombre      = p.Nombre,
        RazonSocial = p.RazonSocial,
        Ruc         = p.Ruc,
        Direccion   = p.Direccion,
        Ciudad      = p.Ciudad,
        SitioWeb    = p.SitioWeb,
        Facebook    = p.Facebook,
        Instagram   = p.Instagram,
        WhatsApp    = p.WhatsApp,
        EsLaboratorio = p.EsLaboratorio,
        IsActive    = p.IsActive,
        CreatedAt   = p.CreatedAt,
        Contactos   = p.Contactos.Select(c => new ProveedorContactoDto
        {
            Id       = c.Id,
            Nombre   = c.Nombre,
            Cargo    = c.Cargo,
            Telefono = c.Telefono,
            Email    = c.Email,
        }).ToList(),
    };
}
