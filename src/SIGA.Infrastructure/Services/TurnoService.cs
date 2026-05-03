using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Turnos;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class TurnoService : ITurnoService
{
    private readonly AppDbContext _db;
    private const int DuracionMinutos = 30;

    public TurnoService(AppDbContext db) => _db = db;

    public async Task<Result<IEnumerable<TurnoResponse>>> GetAllAsync(DateOnly? fecha, int? professionalId, string? estado)
    {
        var query = _db.Turnos
            .Include(t => t.Professional).ThenInclude(p => p.User).ThenInclude(u => u.Person)
            .Include(t => t.Patient).ThenInclude(p => p.Person)
            .AsQueryable();

        if (fecha.HasValue)
        {
            var from = DateTime.SpecifyKind(fecha.Value.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
            var to   = DateTime.SpecifyKind(fecha.Value.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);
            query = query.Where(t => t.FechaHora >= from && t.FechaHora <= to);
        }

        if (professionalId.HasValue)
            query = query.Where(t => t.ProfessionalId == professionalId.Value);

        if (!string.IsNullOrWhiteSpace(estado) && Enum.TryParse<TurnoEstado>(estado, true, out var estadoEnum))
            query = query.Where(t => t.Estado == estadoEnum);

        var turnos = await query.OrderBy(t => t.FechaHora).ToListAsync();
        return Result<IEnumerable<TurnoResponse>>.Success(turnos.Select(ToResponse));
    }

    public async Task<Result<IEnumerable<SlotDisponibleResponse>>> GetSlotsDisponiblesAsync(int professionalId, DateOnly fecha)
    {
        if (await _db.BloqueosFecha.AnyAsync(b => b.ProfessionalId == professionalId && b.Fecha == fecha))
            return Result<IEnumerable<SlotDisponibleResponse>>.Success([]);

        var horario = await _db.HorariosProfesional
            .Include(h => h.Pausas)
            .FirstOrDefaultAsync(h => h.ProfessionalId == professionalId
                                   && h.DiaSemana == fecha.DayOfWeek
                                   && h.Activo);

        if (horario is null)
            return Result<IEnumerable<SlotDisponibleResponse>>.Success([]);

        var allSlots = GenerateSlots(horario.HoraInicio, horario.HoraFin).ToList();

        var slotsLibres = allSlots.Where(slot =>
        {
            var slotFin = slot.AddMinutes(DuracionMinutos);
            return !horario.Pausas.Any(p => slot < p.HoraFin && slotFin > p.HoraInicio);
        }).ToList();

        var from    = DateTime.SpecifyKind(fecha.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);
        var to      = DateTime.SpecifyKind(fecha.ToDateTime(TimeOnly.MaxValue), DateTimeKind.Utc);
        var ocupados = await _db.Turnos
            .Where(t => t.ProfessionalId == professionalId
                     && t.FechaHora >= from
                     && t.FechaHora <= to
                     && t.Estado != TurnoEstado.Cancelado)
            .Select(t => TimeOnly.FromDateTime(t.FechaHora))
            .ToListAsync();

        var ahora      = DateTime.UtcNow;
        var horaActual = fecha == DateOnly.FromDateTime(ahora)
            ? TimeOnly.FromDateTime(ahora)
            : TimeOnly.MinValue;

        var disponibles = slotsLibres
            .Where(s => !ocupados.Contains(s) && s > horaActual)
            .Select(s => new SlotDisponibleResponse { HoraInicio = s, HoraFin = s.AddMinutes(DuracionMinutos) });

        return Result<IEnumerable<SlotDisponibleResponse>>.Success(disponibles);
    }

    public async Task<Result<TurnoResponse>> CreateAsync(CreateTurnoRequest request)
    {
        var professional = await _db.Professionals
            .Include(p => p.User).ThenInclude(u => u.Person)
            .FirstOrDefaultAsync(p => p.Id == request.ProfessionalId);
        if (professional is null)
            return Result<TurnoResponse>.Failure("Profesional no encontrado.", ErrorType.NotFound);

        var patient = await _db.Patients
            .Include(p => p.Person)
            .FirstOrDefaultAsync(p => p.Id == request.PatientId);
        if (patient is null)
            return Result<TurnoResponse>.Failure("Paciente no encontrado.", ErrorType.NotFound);

        if (request.FechaHora < DateTime.UtcNow)
            return Result<TurnoResponse>.Failure("No se pueden reservar turnos en fechas pasadas.", ErrorType.Validation);

        var fecha = DateOnly.FromDateTime(request.FechaHora);
        var hora  = TimeOnly.FromDateTime(request.FechaHora);

        if (await _db.BloqueosFecha.AnyAsync(b => b.ProfessionalId == request.ProfessionalId && b.Fecha == fecha))
            return Result<TurnoResponse>.Failure("El profesional tiene esa fecha bloqueada.", ErrorType.Validation);

        var horario = await _db.HorariosProfesional
            .Include(h => h.Pausas)
            .FirstOrDefaultAsync(h => h.ProfessionalId == request.ProfessionalId
                                   && h.DiaSemana == fecha.DayOfWeek
                                   && h.Activo);

        if (horario is null)
            return Result<TurnoResponse>.Failure("El profesional no trabaja ese día.", ErrorType.Validation);

        var slotFin = hora.AddMinutes(DuracionMinutos);
        if (hora < horario.HoraInicio || slotFin > horario.HoraFin)
            return Result<TurnoResponse>.Failure("El horario está fuera del rango de trabajo del profesional.", ErrorType.Validation);

        if (horario.Pausas.Any(p => hora < p.HoraFin && slotFin > p.HoraInicio))
            return Result<TurnoResponse>.Failure("El horario coincide con una pausa del profesional.", ErrorType.Validation);

        if (await _db.Turnos.AnyAsync(t => t.ProfessionalId == request.ProfessionalId
                                        && t.FechaHora == request.FechaHora
                                        && t.Estado != TurnoEstado.Cancelado))
            return Result<TurnoResponse>.Failure("Ese horario ya está reservado.", ErrorType.Conflict);

        var now   = DateTime.UtcNow;
        var turno = new Turno
        {
            ProfessionalId = request.ProfessionalId,
            PatientId      = request.PatientId,
            FechaHora      = request.FechaHora,
            Estado         = TurnoEstado.Pendiente,
            Motivo         = request.Motivo?.Trim(),
            Notas          = request.Notas?.Trim(),
            CreatedAt      = now,
            UpdatedAt      = now,
        };

        _db.Turnos.Add(turno);
        await _db.SaveChangesAsync();

        turno.Professional = professional;
        turno.Patient      = patient;

        return Result<TurnoResponse>.Success(ToResponse(turno));
    }

    public async Task<Result<TurnoResponse>> UpdateEstadoAsync(int id, UpdateTurnoEstadoRequest request)
    {
        if (!Enum.TryParse<TurnoEstado>(request.Estado, true, out var estadoEnum))
            return Result<TurnoResponse>.Failure("Estado inválido. Valores posibles: Pendiente, Completado, Cancelado.", ErrorType.Validation);

        var turno = await _db.Turnos
            .Include(t => t.Professional).ThenInclude(p => p.User).ThenInclude(u => u.Person)
            .Include(t => t.Patient).ThenInclude(p => p.Person)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (turno is null)
            return Result<TurnoResponse>.Failure("Turno no encontrado.", ErrorType.NotFound);

        turno.Estado    = estadoEnum;
        turno.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Result<TurnoResponse>.Success(ToResponse(turno));
    }

    public async Task<Result<bool>> CancelAsync(int id)
    {
        var turno = await _db.Turnos.FirstOrDefaultAsync(t => t.Id == id);
        if (turno is null)
            return Result<bool>.Failure("Turno no encontrado.", ErrorType.NotFound);

        turno.Estado    = TurnoEstado.Cancelado;
        turno.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return Result<bool>.Success(true);
    }

    private static IEnumerable<TimeOnly> GenerateSlots(TimeOnly inicio, TimeOnly fin)
    {
        var current = inicio;
        while (current.AddMinutes(DuracionMinutos) <= fin)
        {
            yield return current;
            current = current.AddMinutes(DuracionMinutos);
        }
    }

    private static TurnoResponse ToResponse(Turno t) => new()
    {
        Id                 = t.Id,
        ProfessionalId     = t.ProfessionalId,
        ProfessionalNombre = $"{t.Professional.User.Person.FirstName} {t.Professional.User.Person.LastName}",
        PatientId          = t.PatientId,
        PatientNombre      = $"{t.Patient.Person.FirstName} {t.Patient.Person.LastName}",
        FechaHora          = t.FechaHora,
        Estado             = t.Estado.ToString(),
        Motivo             = t.Motivo,
        Notas              = t.Notas,
        CreatedAt          = t.CreatedAt,
        UpdatedAt          = t.UpdatedAt,
    };
}
