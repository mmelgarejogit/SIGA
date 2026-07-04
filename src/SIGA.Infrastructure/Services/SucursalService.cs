using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Sucursales;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class SucursalService : ISucursalService
{
    private readonly AppDbContext _dbContext;

    public SucursalService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<SucursalResponse>>> GetAllAsync(bool soloActivas = false)
    {
        var query = _dbContext.Sucursales
            .Include(s => s.Ciudad)
            .AsQueryable();

        if (soloActivas)
            query = query.Where(s => s.IsActive);

        var sucursales = await query
            .OrderBy(s => s.Nombre)
            .ToListAsync();

        return Result<IEnumerable<SucursalResponse>>.Success(sucursales.Select(ToResponse));
    }

    public async Task<Result<SucursalResponse>> GetByIdAsync(int id)
    {
        var sucursal = await _dbContext.Sucursales
            .Include(s => s.Ciudad)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sucursal is null)
            return Result<SucursalResponse>.Failure("Sucursal no encontrada.", ErrorType.NotFound);

        return Result<SucursalResponse>.Success(ToResponse(sucursal));
    }

    public async Task<Result<SucursalResponse>> CreateAsync(CreateSucursalRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<SucursalResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.Codigo))
            return Result<SucursalResponse>.Failure("El código es obligatorio.", ErrorType.Validation);

        var codigo = request.Codigo.Trim();

        if (await _dbContext.Sucursales.AnyAsync(s => s.Codigo == codigo))
            return Result<SucursalResponse>.Failure("Ya existe una sucursal con ese código.", ErrorType.Conflict);

        var now = DateTime.UtcNow;
        var sucursal = new Sucursal
        {
            Nombre          = request.Nombre.Trim(),
            Codigo          = codigo,
            Direccion       = request.Direccion?.Trim(),
            Telefono        = request.Telefono?.Trim(),
            Email           = request.Email?.Trim(),
            CiudadId        = request.CiudadId,
            Establecimiento = request.Establecimiento?.Trim(),
            IsActive        = true,
            CreatedAt       = now,
            UpdatedAt       = now,
        };

        _dbContext.Sucursales.Add(sucursal);
        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(sucursal.Id);
    }

    public async Task<Result<SucursalResponse>> UpdateAsync(int id, UpdateSucursalRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<SucursalResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (string.IsNullOrWhiteSpace(request.Codigo))
            return Result<SucursalResponse>.Failure("El código es obligatorio.", ErrorType.Validation);

        var sucursal = await _dbContext.Sucursales.FindAsync(id);
        if (sucursal is null)
            return Result<SucursalResponse>.Failure("Sucursal no encontrada.", ErrorType.NotFound);

        var codigo = request.Codigo.Trim();
        if (await _dbContext.Sucursales.AnyAsync(s => s.Codigo == codigo && s.Id != id))
            return Result<SucursalResponse>.Failure("Ya existe una sucursal con ese código.", ErrorType.Conflict);

        sucursal.Nombre          = request.Nombre.Trim();
        sucursal.Codigo          = codigo;
        sucursal.Direccion       = request.Direccion?.Trim();
        sucursal.Telefono        = request.Telefono?.Trim();
        sucursal.Email           = request.Email?.Trim();
        sucursal.CiudadId        = request.CiudadId;
        sucursal.Establecimiento = request.Establecimiento?.Trim();
        sucursal.IsActive        = request.IsActive;
        sucursal.UpdatedAt       = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return await GetByIdAsync(sucursal.Id);
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var sucursal = await _dbContext.Sucursales.FindAsync(id);
        if (sucursal is null)
            return Result<bool>.Failure("Sucursal no encontrada.", ErrorType.NotFound);

        if (!sucursal.IsActive)
            return Result<bool>.Failure("La sucursal ya está inactiva.", ErrorType.Conflict);

        sucursal.IsActive  = false;
        sucursal.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    private static SucursalResponse ToResponse(Sucursal s) => new()
    {
        Id              = s.Id,
        Nombre          = s.Nombre,
        Codigo          = s.Codigo,
        Direccion       = s.Direccion,
        Telefono        = s.Telefono,
        Email           = s.Email,
        CiudadId        = s.CiudadId,
        CiudadNombre    = s.Ciudad?.Nombre,
        Establecimiento = s.Establecimiento,
        IsActive        = s.IsActive,
    };
}
