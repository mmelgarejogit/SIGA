using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Especialidades;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class EspecialidadService : IEspecialidadService
{
    private readonly AppDbContext _dbContext;

    public EspecialidadService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<EspecialidadResponse>>> GetAllAsync()
    {
        var especialidades = await _dbContext.Especialidades
            .OrderBy(e => e.Nombre)
            .ToListAsync();

        return Result<IEnumerable<EspecialidadResponse>>.Success(especialidades.Select(ToResponse));
    }

    public async Task<Result<EspecialidadResponse>> GetByIdAsync(int id)
    {
        var especialidad = await _dbContext.Especialidades.FindAsync(id);

        if (especialidad is null)
            return Result<EspecialidadResponse>.Failure("Especialidad no encontrada.", ErrorType.NotFound);

        return Result<EspecialidadResponse>.Success(ToResponse(especialidad));
    }

    public async Task<Result<EspecialidadResponse>> CreateAsync(CreateEspecialidadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<EspecialidadResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (await _dbContext.Especialidades.AnyAsync(e => e.Nombre == request.Nombre.Trim()))
            return Result<EspecialidadResponse>.Failure("Ya existe una especialidad con ese nombre.", ErrorType.Conflict);

        var especialidad = new Especialidad
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim()
        };

        _dbContext.Especialidades.Add(especialidad);
        await _dbContext.SaveChangesAsync();

        return Result<EspecialidadResponse>.Success(ToResponse(especialidad));
    }

    public async Task<Result<EspecialidadResponse>> UpdateAsync(int id, UpdateEspecialidadRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<EspecialidadResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        var especialidad = await _dbContext.Especialidades.FindAsync(id);

        if (especialidad is null)
            return Result<EspecialidadResponse>.Failure("Especialidad no encontrada.", ErrorType.NotFound);

        if (await _dbContext.Especialidades.AnyAsync(e => e.Nombre == request.Nombre.Trim() && e.Id != id))
            return Result<EspecialidadResponse>.Failure("Ya existe una especialidad con ese nombre.", ErrorType.Conflict);

        especialidad.Nombre = request.Nombre.Trim();
        especialidad.Descripcion = request.Descripcion?.Trim();

        await _dbContext.SaveChangesAsync();

        return Result<EspecialidadResponse>.Success(ToResponse(especialidad));
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var especialidad = await _dbContext.Especialidades
            .Include(e => e.Profesionales)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (especialidad is null)
            return Result<bool>.Failure("Especialidad no encontrada.", ErrorType.NotFound);

        if (especialidad.Profesionales.Count > 0)
            return Result<bool>.Failure(
                "No se puede eliminar la especialidad porque está asignada a uno o más profesionales.",
                ErrorType.Conflict);

        _dbContext.Especialidades.Remove(especialidad);
        await _dbContext.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    private static EspecialidadResponse ToResponse(Especialidad e) => new()
    {
        Id = e.Id,
        Nombre = e.Nombre,
        Descripcion = e.Descripcion
    };
}
