using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class ServicioService(AppDbContext db) : IServicioService
{
    private IQueryable<Servicio> ServiciosConTarifas() =>
        db.Servicios
            .Include(s => s.Tarifas).ThenInclude(t => t.Professional).ThenInclude(p => p!.User).ThenInclude(u => u.Person)
            .Include(s => s.Tarifas).ThenInclude(t => t.Especialidad);

    public async Task<Result<IEnumerable<ServicioDto>>> GetAllAsync()
    {
        var items = await ServiciosConTarifas().OrderBy(s => s.Nombre).ToListAsync();
        return Result<IEnumerable<ServicioDto>>.Success(items.Select(ToDto));
    }

    public async Task<Result<ServicioDto>> CreateAsync(CreateServicioRequest request)
    {
        var nombre = request.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<ServicioDto>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (await db.Servicios.AnyAsync(s => s.Nombre == nombre))
            return Result<ServicioDto>.Failure("Ya existe un servicio con ese nombre.", ErrorType.Conflict);

        if (request.Precio < 0)
            return Result<ServicioDto>.Failure("El precio no puede ser negativo.", ErrorType.Validation);

        var now = DateTime.UtcNow;
        var item = new Servicio
        {
            Nombre      = nombre,
            Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim(),
            Precio      = request.Precio,
            CreatedAt   = now,
            UpdatedAt   = now,
        };
        db.Servicios.Add(item);
        await db.SaveChangesAsync();
        return Result<ServicioDto>.Success(ToDto(item));
    }

    public async Task<Result<ServicioDto>> UpdateAsync(int id, UpdateServicioRequest request)
    {
        var item = await ServiciosConTarifas().FirstOrDefaultAsync(s => s.Id == id);
        if (item is null) return Result<ServicioDto>.Failure("Servicio no encontrado.", ErrorType.NotFound);

        var nombre = request.Nombre.Trim();
        if (string.IsNullOrWhiteSpace(nombre))
            return Result<ServicioDto>.Failure("El nombre es obligatorio.", ErrorType.Validation);

        if (await db.Servicios.AnyAsync(s => s.Nombre == nombre && s.Id != id))
            return Result<ServicioDto>.Failure("Ya existe un servicio con ese nombre.", ErrorType.Conflict);

        if (request.Precio < 0)
            return Result<ServicioDto>.Failure("El precio no puede ser negativo.", ErrorType.Validation);

        item.Nombre      = nombre;
        item.Descripcion = string.IsNullOrWhiteSpace(request.Descripcion) ? null : request.Descripcion.Trim();
        item.Precio      = request.Precio;
        item.IsActive    = request.IsActive;
        item.UpdatedAt   = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Result<ServicioDto>.Success(ToDto(item));
    }

    public async Task<Result<bool>> DeactivateAsync(int id)
    {
        var item = await db.Servicios.FindAsync(id);
        if (item is null) return Result<bool>.Failure("Servicio no encontrado.", ErrorType.NotFound);
        item.IsActive  = false;
        item.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    // ── Tarifas ──────────────────────────────────────────────────────────────

    public async Task<Result<ServicioDto>> AddTarifaAsync(int servicioId, CreateServicioTarifaRequest request)
    {
        var servicio = await db.Servicios.FindAsync(servicioId);
        if (servicio is null) return Result<ServicioDto>.Failure("Servicio no encontrado.", ErrorType.NotFound);

        var tieneProfesional = request.ProfessionalId.HasValue;
        var tieneEspecialidad = request.EspecialidadId.HasValue;

        if (tieneProfesional == tieneEspecialidad)
            return Result<ServicioDto>.Failure("Indicá un profesional o una especialidad (exactamente uno).", ErrorType.Validation);

        if (request.Precio < 0)
            return Result<ServicioDto>.Failure("El precio no puede ser negativo.", ErrorType.Validation);

        if (tieneProfesional)
        {
            if (!await db.Professionals.AnyAsync(p => p.Id == request.ProfessionalId!.Value))
                return Result<ServicioDto>.Failure("El profesional no existe.", ErrorType.Validation);
            if (await db.ServicioTarifas.AnyAsync(t => t.ServicioId == servicioId && t.ProfessionalId == request.ProfessionalId))
                return Result<ServicioDto>.Failure("Ya hay una tarifa para ese profesional en este servicio.", ErrorType.Conflict);
        }
        else
        {
            if (!await db.Especialidades.AnyAsync(e => e.Id == request.EspecialidadId!.Value))
                return Result<ServicioDto>.Failure("La especialidad no existe.", ErrorType.Validation);
            if (await db.ServicioTarifas.AnyAsync(t => t.ServicioId == servicioId && t.EspecialidadId == request.EspecialidadId))
                return Result<ServicioDto>.Failure("Ya hay una tarifa para esa especialidad en este servicio.", ErrorType.Conflict);
        }

        var now = DateTime.UtcNow;
        db.ServicioTarifas.Add(new ServicioTarifa
        {
            ServicioId     = servicioId,
            ProfessionalId = request.ProfessionalId,
            EspecialidadId = request.EspecialidadId,
            Precio         = request.Precio,
            CreatedAt      = now,
            UpdatedAt      = now,
        });
        await db.SaveChangesAsync();

        var actualizado = await ServiciosConTarifas().FirstAsync(s => s.Id == servicioId);
        return Result<ServicioDto>.Success(ToDto(actualizado));
    }

    public async Task<Result<bool>> RemoveTarifaAsync(int tarifaId)
    {
        var tarifa = await db.ServicioTarifas.FindAsync(tarifaId);
        if (tarifa is null) return Result<bool>.Failure("Tarifa no encontrada.", ErrorType.NotFound);
        db.ServicioTarifas.Remove(tarifa);
        await db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    public async Task<Result<PrecioResueltoDto>> ResolvePrecioAsync(int servicioId, int? professionalId)
    {
        var servicio = await db.Servicios
            .Include(s => s.Tarifas)
            .FirstOrDefaultAsync(s => s.Id == servicioId);
        if (servicio is null) return Result<PrecioResueltoDto>.Failure("Servicio no encontrado.", ErrorType.NotFound);

        if (professionalId.HasValue)
        {
            // 1) Tarifa específica del profesional
            var porProfesional = servicio.Tarifas.FirstOrDefault(t => t.ProfessionalId == professionalId);
            if (porProfesional is not null)
                return Result<PrecioResueltoDto>.Success(new PrecioResueltoDto { Precio = porProfesional.Precio, Origen = "profesional" });

            // 2) Tarifa por especialidad del profesional
            var especialidadIds = await db.ProfesionalEspecialidades
                .Where(pe => pe.ProfessionalId == professionalId)
                .Select(pe => pe.EspecialidadId)
                .ToListAsync();

            var porEspecialidad = servicio.Tarifas
                .FirstOrDefault(t => t.EspecialidadId.HasValue && especialidadIds.Contains(t.EspecialidadId.Value));
            if (porEspecialidad is not null)
                return Result<PrecioResueltoDto>.Success(new PrecioResueltoDto { Precio = porEspecialidad.Precio, Origen = "especialidad" });
        }

        // 3) Precio base
        return Result<PrecioResueltoDto>.Success(new PrecioResueltoDto { Precio = servicio.Precio, Origen = "base" });
    }

    // ── Mapeo ────────────────────────────────────────────────────────────────

    private static ServicioDto ToDto(Servicio s) => new()
    {
        Id          = s.Id,
        Nombre      = s.Nombre,
        Descripcion = s.Descripcion,
        Precio      = s.Precio,
        IsActive    = s.IsActive,
        CreatedAt   = s.CreatedAt,
        Tarifas     = s.Tarifas
            .OrderBy(t => t.ProfessionalId.HasValue ? 0 : 1)
            .Select(ToTarifaDto)
            .ToList(),
    };

    private static ServicioTarifaDto ToTarifaDto(ServicioTarifa t) => new()
    {
        Id                 = t.Id,
        ProfessionalId     = t.ProfessionalId,
        ProfessionalNombre = t.Professional?.User.Person is { } p ? $"{p.FirstName} {p.LastName}" : null,
        EspecialidadId     = t.EspecialidadId,
        EspecialidadNombre = t.Especialidad?.Nombre,
        Precio             = t.Precio,
    };
}
