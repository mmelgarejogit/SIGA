using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class MotivoMovimientoService(AppDbContext db) : IMotivoMovimientoService
{
    public async Task<Result<IEnumerable<MotivoMovimientoResponse>>> GetAllAsync(string? tipo)
    {
        var query = db.MotivosMovimiento.AsQueryable();

        if (!string.IsNullOrWhiteSpace(tipo) && Enum.TryParse<TipoMotivoMovimiento>(tipo, ignoreCase: true, out var t))
            query = query.Where(m => m.Tipo == t || m.Tipo == TipoMotivoMovimiento.Ambos);

        var items = await query
            .OrderBy(m => m.Nombre)
            .Select(m => ToResponse(m))
            .ToListAsync();

        return Result<IEnumerable<MotivoMovimientoResponse>>.Success(items);
    }

    public async Task<Result<MotivoMovimientoResponse>> CreateAsync(CreateMotivoMovimientoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<MotivoMovimientoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (!Enum.TryParse<TipoMotivoMovimiento>(request.Tipo, ignoreCase: true, out var tipoCreate))
            return Result<MotivoMovimientoResponse>.Failure("Tipo inválido. Use Entrada, Salida o Ambos.", ErrorType.Validation);

        var nombre = request.Nombre.Trim();
        if (await db.MotivosMovimiento.AnyAsync(m => m.Nombre.ToLower() == nombre.ToLower()))
            return Result<MotivoMovimientoResponse>.Failure("Ya existe un motivo con ese nombre.", ErrorType.Conflict);

        var motivo = new MotivoMovimiento
        {
            Nombre = nombre,
            Tipo   = tipoCreate,
        };

        db.MotivosMovimiento.Add(motivo);
        await db.SaveChangesAsync();

        return Result<MotivoMovimientoResponse>.Success(ToResponse(motivo));
    }

    public async Task<Result<MotivoMovimientoResponse>> UpdateAsync(int id, UpdateMotivoMovimientoRequest request)
    {
        var motivo = await db.MotivosMovimiento.FindAsync(id);
        if (motivo is null)
            return Result<MotivoMovimientoResponse>.Failure("Motivo no encontrado.", ErrorType.NotFound);

        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result<MotivoMovimientoResponse>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (!Enum.TryParse<TipoMotivoMovimiento>(request.Tipo, ignoreCase: true, out var tipoUpdate))
            return Result<MotivoMovimientoResponse>.Failure("Tipo inválido. Use Entrada, Salida o Ambos.", ErrorType.Validation);

        var nombre = request.Nombre.Trim();
        if (await db.MotivosMovimiento.AnyAsync(m => m.Nombre.ToLower() == nombre.ToLower() && m.Id != id))
            return Result<MotivoMovimientoResponse>.Failure("Ya existe un motivo con ese nombre.", ErrorType.Conflict);

        motivo.Nombre   = nombre;
        motivo.Tipo     = tipoUpdate;
        motivo.IsActive = request.IsActive;

        await db.SaveChangesAsync();
        return Result<MotivoMovimientoResponse>.Success(ToResponse(motivo));
    }

    public async Task<Result<bool>> DeactivateAsync(int id)
    {
        var motivo = await db.MotivosMovimiento.FindAsync(id);
        if (motivo is null)
            return Result<bool>.Failure("Motivo no encontrado.", ErrorType.NotFound);

        motivo.IsActive = false;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static MotivoMovimientoResponse ToResponse(MotivoMovimiento m) => new()
    {
        Id        = m.Id,
        Nombre    = m.Nombre,
        Tipo      = m.Tipo.ToString(),
        IsActive  = m.IsActive,
        CreatedAt = m.CreatedAt,
    };
}
