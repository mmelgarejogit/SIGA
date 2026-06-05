using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Ubicacion;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class UbicacionService(AppDbContext db) : IUbicacionService
{
    // ── Departamentos ─────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<DepartamentoResponse>>> GetDepartamentosAsync(bool? isActive)
    {
        var query = db.Departamentos.AsQueryable();
        if (isActive.HasValue) query = query.Where(d => d.IsActive == isActive.Value);

        var counts = await db.Ciudades
            .GroupBy(c => c.DepartamentoId)
            .Select(g => new { g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Total);

        var departamentos = await query.OrderBy(d => d.Nombre).ToListAsync();

        return Result<IEnumerable<DepartamentoResponse>>.Success(
            departamentos.Select(d => new DepartamentoResponse
            {
                Id            = d.Id,
                Nombre        = d.Nombre,
                IsActive      = d.IsActive,
                TotalCiudades = counts.GetValueOrDefault(d.Id, 0),
            }));
    }

    public async Task<Result<DepartamentoResponse>> CreateDepartamentoAsync(CreateDepartamentoRequest request)
    {
        var nombre = request.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<DepartamentoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (await db.Departamentos.AnyAsync(d => d.Nombre == nombre))
            return Result<DepartamentoResponse>.Failure("Ya existe un departamento con ese nombre.", ErrorType.Conflict);

        var dep = new Departamento { Nombre = nombre };
        db.Departamentos.Add(dep);
        await db.SaveChangesAsync();

        return Result<DepartamentoResponse>.Success(new DepartamentoResponse
        {
            Id = dep.Id, Nombre = dep.Nombre, IsActive = dep.IsActive, TotalCiudades = 0,
        });
    }

    public async Task<Result<DepartamentoResponse>> UpdateDepartamentoAsync(int id, UpdateDepartamentoRequest request)
    {
        var dep = await db.Departamentos.FindAsync(id);
        if (dep is null)
            return Result<DepartamentoResponse>.Failure("Departamento no encontrado.", ErrorType.NotFound);

        var nombre = request.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<DepartamentoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (await db.Departamentos.AnyAsync(d => d.Nombre == nombre && d.Id != id))
            return Result<DepartamentoResponse>.Failure("Ya existe un departamento con ese nombre.", ErrorType.Conflict);

        dep.Nombre   = nombre;
        dep.IsActive = request.IsActive;
        await db.SaveChangesAsync();

        var total = await db.Ciudades.CountAsync(c => c.DepartamentoId == id);
        return Result<DepartamentoResponse>.Success(new DepartamentoResponse
        {
            Id = dep.Id, Nombre = dep.Nombre, IsActive = dep.IsActive, TotalCiudades = total,
        });
    }

    // ── Ciudades ──────────────────────────────────────────────────────────────────

    public async Task<Result<IEnumerable<CiudadResponse>>> GetCiudadesAsync(int? departamentoId, bool? isActive)
    {
        var query = db.Ciudades.Include(c => c.Departamento).AsQueryable();
        if (departamentoId.HasValue) query = query.Where(c => c.DepartamentoId == departamentoId.Value);
        if (isActive.HasValue) query = query.Where(c => c.IsActive == isActive.Value);

        var ciudades = await query
            .OrderBy(c => c.Departamento.Nombre)
            .ThenBy(c => c.Nombre)
            .ToListAsync();

        return Result<IEnumerable<CiudadResponse>>.Success(
            ciudades.Select(c => new CiudadResponse
            {
                Id                  = c.Id,
                Nombre              = c.Nombre,
                DepartamentoId      = c.DepartamentoId,
                DepartamentoNombre  = c.Departamento.Nombre,
                IsActive            = c.IsActive,
            }));
    }

    public async Task<Result<CiudadResponse>> CreateCiudadAsync(CreateCiudadRequest request)
    {
        var nombre = request.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<CiudadResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        var dep = await db.Departamentos.FindAsync(request.DepartamentoId);
        if (dep is null)
            return Result<CiudadResponse>.Failure("Departamento no encontrado.", ErrorType.NotFound);

        if (await db.Ciudades.AnyAsync(c => c.Nombre == nombre && c.DepartamentoId == request.DepartamentoId))
            return Result<CiudadResponse>.Failure("Ya existe una ciudad con ese nombre en el departamento.", ErrorType.Conflict);

        var ciudad = new Ciudad { Nombre = nombre, DepartamentoId = request.DepartamentoId };
        db.Ciudades.Add(ciudad);
        await db.SaveChangesAsync();

        return Result<CiudadResponse>.Success(new CiudadResponse
        {
            Id = ciudad.Id, Nombre = ciudad.Nombre,
            DepartamentoId = dep.Id, DepartamentoNombre = dep.Nombre, IsActive = ciudad.IsActive,
        });
    }

    public async Task<Result<CiudadResponse>> UpdateCiudadAsync(int id, UpdateCiudadRequest request)
    {
        var ciudad = await db.Ciudades.Include(c => c.Departamento).FirstOrDefaultAsync(c => c.Id == id);
        if (ciudad is null)
            return Result<CiudadResponse>.Failure("Ciudad no encontrada.", ErrorType.NotFound);

        var nombre = request.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<CiudadResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        var dep = await db.Departamentos.FindAsync(request.DepartamentoId);
        if (dep is null)
            return Result<CiudadResponse>.Failure("Departamento no encontrado.", ErrorType.NotFound);

        if (await db.Ciudades.AnyAsync(c => c.Nombre == nombre && c.DepartamentoId == request.DepartamentoId && c.Id != id))
            return Result<CiudadResponse>.Failure("Ya existe una ciudad con ese nombre en el departamento.", ErrorType.Conflict);

        ciudad.Nombre         = nombre;
        ciudad.DepartamentoId = request.DepartamentoId;
        ciudad.IsActive       = request.IsActive;
        await db.SaveChangesAsync();

        return Result<CiudadResponse>.Success(new CiudadResponse
        {
            Id = ciudad.Id, Nombre = ciudad.Nombre,
            DepartamentoId = dep.Id, DepartamentoNombre = dep.Nombre, IsActive = ciudad.IsActive,
        });
    }
}
