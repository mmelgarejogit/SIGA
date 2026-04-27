using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Horarios;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class HorarioProfesionalService : IHorarioProfesionalService
{
    private readonly AppDbContext _dbContext;

    public HorarioProfesionalService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<IEnumerable<HorarioProfesionalResponse>>> GetHorariosAsync(int professionalId)
    {
        if (!await _dbContext.Professionals.AnyAsync(p => p.Id == professionalId))
            return Result<IEnumerable<HorarioProfesionalResponse>>.Failure("Profesional no encontrado.", ErrorType.NotFound);

        var horarios = await _dbContext.HorariosProfesional
            .Include(h => h.Pausas)
            .Where(h => h.ProfessionalId == professionalId)
            .OrderBy(h => h.DiaSemana)
            .ToListAsync();

        return Result<IEnumerable<HorarioProfesionalResponse>>.Success(horarios.Select(ToResponse));
    }

    public async Task<Result<IEnumerable<HorarioProfesionalResponse>>> SetHorariosAsync(int professionalId, SetHorariosRequest request)
    {
        if (!await _dbContext.Professionals.AnyAsync(p => p.Id == professionalId))
            return Result<IEnumerable<HorarioProfesionalResponse>>.Failure("Profesional no encontrado.", ErrorType.NotFound);

        var validationError = ValidateHorarios(request.Horarios);
        if (validationError is not null)
            return Result<IEnumerable<HorarioProfesionalResponse>>.Failure(validationError, ErrorType.Validation);

        var existing = await _dbContext.HorariosProfesional
            .Include(h => h.Pausas)
            .Where(h => h.ProfessionalId == professionalId)
            .ToListAsync();

        _dbContext.HorariosProfesional.RemoveRange(existing);

        var nuevos = request.Horarios.Select(h => new HorarioProfesional
        {
            ProfessionalId = professionalId,
            DiaSemana = h.DiaSemana,
            HoraInicio = h.HoraInicio,
            HoraFin = h.HoraFin,
            Activo = h.Activo,
            Pausas = h.Pausas.Select(p => new PausaHorario
            {
                HoraInicio = p.HoraInicio,
                HoraFin = p.HoraFin,
                Descripcion = p.Descripcion?.Trim()
            }).ToList()
        }).ToList();

        _dbContext.HorariosProfesional.AddRange(nuevos);
        await _dbContext.SaveChangesAsync();

        return Result<IEnumerable<HorarioProfesionalResponse>>.Success(nuevos.Select(ToResponse));
    }

    public async Task<Result<IEnumerable<BloqueoFechaResponse>>> GetBloqueosAsync(int professionalId)
    {
        if (!await _dbContext.Professionals.AnyAsync(p => p.Id == professionalId))
            return Result<IEnumerable<BloqueoFechaResponse>>.Failure("Profesional no encontrado.", ErrorType.NotFound);

        var bloqueos = await _dbContext.BloqueosFecha
            .Where(b => b.ProfessionalId == professionalId)
            .OrderBy(b => b.Fecha)
            .ToListAsync();

        return Result<IEnumerable<BloqueoFechaResponse>>.Success(bloqueos.Select(ToBloqueoResponse));
    }

    public async Task<Result<BloqueoFechaResponse>> AddBloqueoAsync(int professionalId, BloqueoFechaRequest request)
    {
        if (!await _dbContext.Professionals.AnyAsync(p => p.Id == professionalId))
            return Result<BloqueoFechaResponse>.Failure("Profesional no encontrado.", ErrorType.NotFound);

        if (await _dbContext.BloqueosFecha.AnyAsync(b => b.ProfessionalId == professionalId && b.Fecha == request.Fecha))
            return Result<BloqueoFechaResponse>.Failure("Ya existe un bloqueo para esa fecha.", ErrorType.Conflict);

        var bloqueo = new BloqueoFecha
        {
            ProfessionalId = professionalId,
            Fecha = request.Fecha,
            Motivo = request.Motivo?.Trim()
        };

        _dbContext.BloqueosFecha.Add(bloqueo);
        await _dbContext.SaveChangesAsync();

        return Result<BloqueoFechaResponse>.Success(ToBloqueoResponse(bloqueo));
    }

    public async Task<Result<bool>> RemoveBloqueoAsync(int professionalId, int bloqueoId)
    {
        var bloqueo = await _dbContext.BloqueosFecha
            .FirstOrDefaultAsync(b => b.Id == bloqueoId && b.ProfessionalId == professionalId);

        if (bloqueo is null)
            return Result<bool>.Failure("Bloqueo no encontrado.", ErrorType.NotFound);

        _dbContext.BloqueosFecha.Remove(bloqueo);
        await _dbContext.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    private static string? ValidateHorarios(List<HorarioDiaRequest> horarios)
    {
        var diasDuplicados = horarios.GroupBy(h => h.DiaSemana).Any(g => g.Count() > 1);
        if (diasDuplicados)
            return "No puede haber dos horarios para el mismo día.";

        foreach (var h in horarios)
        {
            if (h.HoraFin <= h.HoraInicio)
                return $"La hora de fin debe ser posterior a la hora de inicio ({h.DiaSemana}).";

            foreach (var p in h.Pausas)
            {
                if (p.HoraFin <= p.HoraInicio)
                    return $"La hora de fin de una pausa debe ser posterior a su hora de inicio ({h.DiaSemana}).";

                if (p.HoraInicio < h.HoraInicio || p.HoraFin > h.HoraFin)
                    return $"Las pausas deben estar dentro del rango horario del día ({h.DiaSemana}).";
            }

            var pausasOrdenadas = h.Pausas.OrderBy(p => p.HoraInicio).ToList();
            for (var i = 1; i < pausasOrdenadas.Count; i++)
            {
                if (pausasOrdenadas[i].HoraInicio < pausasOrdenadas[i - 1].HoraFin)
                    return $"Las pausas no pueden superponerse ({h.DiaSemana}).";
            }
        }

        return null;
    }

    private static HorarioProfesionalResponse ToResponse(HorarioProfesional h) => new()
    {
        Id = h.Id,
        DiaSemana = h.DiaSemana,
        HoraInicio = h.HoraInicio,
        HoraFin = h.HoraFin,
        Activo = h.Activo,
        Pausas = h.Pausas.Select(p => new PausaResponse
        {
            Id = p.Id,
            HoraInicio = p.HoraInicio,
            HoraFin = p.HoraFin,
            Descripcion = p.Descripcion
        }).ToList()
    };

    private static BloqueoFechaResponse ToBloqueoResponse(BloqueoFecha b) => new()
    {
        Id = b.Id,
        Fecha = b.Fecha,
        Motivo = b.Motivo
    };
}
