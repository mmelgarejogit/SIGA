using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Turnos;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Options;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class TurnoService : ITurnoService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _email;
    private readonly string _frontendUrl;
    private const int DuracionMinutos = 30;

    public TurnoService(AppDbContext db, IEmailService email, IOptions<AppOptions> appOptions)
    {
        _db          = db;
        _email       = email;
        _frontendUrl = appOptions.Value.FrontendUrl;
    }

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

        var validationError = await ValidateSlotAsync(request.ProfessionalId, request.FechaHora);
        if (validationError is not null)
            return Result<TurnoResponse>.Failure(validationError, ErrorType.Validation);

        var now   = DateTime.UtcNow;
        var token = Guid.NewGuid().ToString("N");
        var turno = new Turno
        {
            ProfessionalId    = request.ProfessionalId,
            PatientId         = request.PatientId,
            FechaHora         = request.FechaHora,
            Estado            = TurnoEstado.Pendiente,
            Motivo            = request.Motivo?.Trim(),
            Notas             = request.Notas?.Trim(),
            ConfirmationToken = token,
            CreatedAt         = now,
            UpdatedAt         = now,
        };

        _db.Turnos.Add(turno);
        await _db.SaveChangesAsync();

        turno.Professional = professional;
        turno.Patient      = patient;

        await TrySendEmailAsync(patient.Person, professional.User.Person,
            turno.FechaHora, turno.Motivo, TurnoEmailTipo.Registrado, token);

        return Result<TurnoResponse>.Success(ToResponse(turno));
    }

    public async Task<Result<TurnoResponse>> UpdateEstadoAsync(int id, UpdateTurnoEstadoRequest request)
    {
        if (!Enum.TryParse<TurnoEstado>(request.Estado, true, out var estadoEnum))
            return Result<TurnoResponse>.Failure(
                "Estado inválido. Valores posibles: Pendiente, Confirmado, Completado, Cancelado.",
                ErrorType.Validation);

        var turno = await _db.Turnos
            .Include(t => t.Professional).ThenInclude(p => p.User).ThenInclude(u => u.Person)
            .Include(t => t.Patient).ThenInclude(p => p.Person)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (turno is null)
            return Result<TurnoResponse>.Failure("Turno no encontrado.", ErrorType.NotFound);

        var estadoAnterior = turno.Estado;
        turno.Estado    = estadoEnum;
        turno.UpdatedAt = DateTime.UtcNow;

        if (estadoEnum == TurnoEstado.Confirmado)
            turno.ConfirmationToken = null;

        await _db.SaveChangesAsync();

        if (estadoEnum != estadoAnterior)
        {
            if (estadoEnum == TurnoEstado.Confirmado)
                await TrySendEmailAsync(turno.Patient.Person, turno.Professional.User.Person,
                    turno.FechaHora, turno.Motivo, TurnoEmailTipo.Confirmado);

            else if (estadoEnum == TurnoEstado.Cancelado)
                await TrySendEmailAsync(turno.Patient.Person, turno.Professional.User.Person,
                    turno.FechaHora, turno.Motivo, TurnoEmailTipo.Cancelado);
        }

        return Result<TurnoResponse>.Success(ToResponse(turno));
    }

    public async Task<Result<bool>> CancelAsync(int id)
    {
        var turno = await _db.Turnos
            .Include(t => t.Professional).ThenInclude(p => p.User).ThenInclude(u => u.Person)
            .Include(t => t.Patient).ThenInclude(p => p.Person)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (turno is null)
            return Result<bool>.Failure("Turno no encontrado.", ErrorType.NotFound);

        turno.Estado               = TurnoEstado.Cancelado;
        turno.SolicitudCancelacion = false;
        turno.ConfirmationToken    = null;
        turno.UpdatedAt            = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await TrySendEmailAsync(turno.Patient.Person, turno.Professional.User.Person,
            turno.FechaHora, turno.Motivo, TurnoEmailTipo.Cancelado);

        return Result<bool>.Success(true);
    }

    public async Task<Result<TurnoResponse>> ConfirmarPorTokenAsync(string token)
    {
        var turno = await _db.Turnos
            .Include(t => t.Professional).ThenInclude(p => p.User).ThenInclude(u => u.Person)
            .Include(t => t.Patient).ThenInclude(p => p.Person)
            .FirstOrDefaultAsync(t => t.ConfirmationToken == token);

        if (turno is null)
            return Result<TurnoResponse>.Failure(
                "El enlace de confirmación no es válido o ya fue utilizado.", ErrorType.NotFound);

        if (turno.Estado == TurnoEstado.Cancelado)
            return Result<TurnoResponse>.Failure("Este turno fue cancelado.", ErrorType.Conflict);

        if (turno.Estado == TurnoEstado.Confirmado)
            return Result<TurnoResponse>.Failure("Este turno ya está confirmado.", ErrorType.Conflict);

        turno.Estado            = TurnoEstado.Confirmado;
        turno.ConfirmationToken = null;
        turno.UpdatedAt         = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await TrySendEmailAsync(turno.Patient.Person, turno.Professional.User.Person,
            turno.FechaHora, turno.Motivo, TurnoEmailTipo.Confirmado);

        return Result<TurnoResponse>.Success(ToResponse(turno));
    }

    // ── Patient self-scheduling ───────────────────────────────────────────

    private async Task<int?> ResolvePatientIdAsync(int userId)
    {
        var patient = await _db.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
        return patient?.Id;
    }

    public async Task<Result<IEnumerable<TurnoResponse>>> GetMisTurnosAsync(int userId)
    {
        var patientId = await ResolvePatientIdAsync(userId);
        if (!patientId.HasValue)
            return Result<IEnumerable<TurnoResponse>>.Failure("No se encontró perfil de paciente asociado a tu cuenta.", ErrorType.NotFound);

        var turnos = await _db.Turnos
            .Include(t => t.Professional).ThenInclude(p => p.User).ThenInclude(u => u.Person)
            .Include(t => t.Patient).ThenInclude(p => p.Person)
            .Where(t => t.PatientId == patientId.Value)
            .OrderByDescending(t => t.FechaHora)
            .ToListAsync();

        return Result<IEnumerable<TurnoResponse>>.Success(turnos.Select(ToResponse));
    }

    public async Task<Result<TurnoResponse>> SelfBookAsync(int userId, SelfBookTurnoRequest request)
    {
        var patientId = await ResolvePatientIdAsync(userId);
        if (!patientId.HasValue)
            return Result<TurnoResponse>.Failure("No se encontró perfil de paciente asociado a tu cuenta.", ErrorType.NotFound);

        var tieneActivo = await _db.Turnos.AnyAsync(t =>
            t.PatientId == patientId.Value &&
            t.Estado != TurnoEstado.Cancelado);

        if (tieneActivo)
            return Result<TurnoResponse>.Failure("Ya tenés un turno activo. Cancelá o completá el turno existente antes de reservar uno nuevo.", ErrorType.Conflict);

        var professional = await _db.Professionals
            .Include(p => p.User).ThenInclude(u => u.Person)
            .FirstOrDefaultAsync(p => p.Id == request.ProfessionalId);
        if (professional is null)
            return Result<TurnoResponse>.Failure("Profesional no encontrado.", ErrorType.NotFound);

        var patient = await _db.Patients
            .Include(p => p.Person)
            .FirstOrDefaultAsync(p => p.Id == patientId.Value);
        if (patient is null)
            return Result<TurnoResponse>.Failure("Paciente no encontrado.", ErrorType.NotFound);

        var validationError = await ValidateSlotAsync(request.ProfessionalId, request.FechaHora);
        if (validationError is not null)
            return Result<TurnoResponse>.Failure(validationError, ErrorType.Validation);

        var now   = DateTime.UtcNow;
        var token = Guid.NewGuid().ToString("N");
        var turno = new Turno
        {
            ProfessionalId    = request.ProfessionalId,
            PatientId         = patientId.Value,
            FechaHora         = request.FechaHora,
            Estado            = TurnoEstado.Pendiente,
            SolicitudCancelacion = false,
            Motivo            = request.Motivo?.Trim(),
            ConfirmationToken = token,
            CreatedAt         = now,
            UpdatedAt         = now,
        };

        _db.Turnos.Add(turno);
        await _db.SaveChangesAsync();

        turno.Professional = professional;
        turno.Patient      = patient;

        await TrySendEmailAsync(patient.Person, professional.User.Person,
            turno.FechaHora, turno.Motivo, TurnoEmailTipo.Registrado, token);

        return Result<TurnoResponse>.Success(ToResponse(turno));
    }

    public async Task<Result<bool>> SolicitarCancelacionAsync(int turnoId, int userId)
    {
        var patientId = await ResolvePatientIdAsync(userId);
        if (!patientId.HasValue)
            return Result<bool>.Failure("No se encontró perfil de paciente asociado a tu cuenta.", ErrorType.NotFound);

        var turno = await _db.Turnos.FirstOrDefaultAsync(t => t.Id == turnoId && t.PatientId == patientId.Value);
        if (turno is null)
            return Result<bool>.Failure("Turno no encontrado.", ErrorType.NotFound);

        if (turno.Estado == TurnoEstado.Cancelado)
            return Result<bool>.Failure("El turno ya está cancelado.", ErrorType.Conflict);

        if (turno.Estado == TurnoEstado.Completado)
            return Result<bool>.Failure("No se puede solicitar cancelación de un turno completado.", ErrorType.Conflict);

        if (turno.SolicitudCancelacion)
            return Result<bool>.Failure("Ya solicitaste la cancelación de este turno. Esperá la confirmación del equipo.", ErrorType.Conflict);

        turno.SolicitudCancelacion = true;
        turno.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> GestionarCancelacionAsync(int turnoId, GestionarCancelacionRequest request)
    {
        var turno = await _db.Turnos
            .Include(t => t.Professional).ThenInclude(p => p.User).ThenInclude(u => u.Person)
            .Include(t => t.Patient).ThenInclude(p => p.Person)
            .FirstOrDefaultAsync(t => t.Id == turnoId);

        if (turno is null)
            return Result<bool>.Failure("Turno no encontrado.", ErrorType.NotFound);

        if (!turno.SolicitudCancelacion)
            return Result<bool>.Failure("Este turno no tiene una solicitud de cancelación pendiente.", ErrorType.Validation);

        if (request.Aprobar)
        {
            turno.Estado            = TurnoEstado.Cancelado;
            turno.ConfirmationToken = null;

            await TrySendEmailAsync(turno.Patient.Person, turno.Professional.User.Person,
                turno.FechaHora, turno.Motivo, TurnoEmailTipo.Cancelado);
        }

        turno.SolicitudCancelacion = false;
        turno.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private async Task<string?> ValidateSlotAsync(int professionalId, DateTime fechaHora)
    {
        if (fechaHora < DateTime.UtcNow)
            return "No se pueden reservar turnos en fechas pasadas.";

        var fecha = DateOnly.FromDateTime(fechaHora);
        var hora  = TimeOnly.FromDateTime(fechaHora);

        if (await _db.BloqueosFecha.AnyAsync(b => b.ProfessionalId == professionalId && b.Fecha == fecha))
            return "El profesional tiene esa fecha bloqueada.";

        var horario = await _db.HorariosProfesional
            .Include(h => h.Pausas)
            .FirstOrDefaultAsync(h => h.ProfessionalId == professionalId
                                   && h.DiaSemana == fecha.DayOfWeek
                                   && h.Activo);

        if (horario is null) return "El profesional no trabaja ese día.";

        var slotFin = hora.AddMinutes(DuracionMinutos);
        if (hora < horario.HoraInicio || slotFin > horario.HoraFin)
            return "El horario está fuera del rango de trabajo del profesional.";

        if (horario.Pausas.Any(p => hora < p.HoraFin && slotFin > p.HoraInicio))
            return "El horario coincide con una pausa del profesional.";

        if (await _db.Turnos.AnyAsync(t => t.ProfessionalId == professionalId
                                        && t.FechaHora == fechaHora
                                        && t.Estado != TurnoEstado.Cancelado))
            return "Ese horario ya está reservado.";

        return null;
    }

    private async Task TrySendEmailAsync(
        Person pacientePerson, Person profPerson,
        DateTime fechaHora, string? motivo,
        TurnoEmailTipo tipo, string? token = null)
    {
        if (string.IsNullOrWhiteSpace(pacientePerson.Email)) return;

        try
        {
            var (subject, body) = BuildTurnoEmail(
                pacientePerson.FirstName + " " + pacientePerson.LastName,
                profPerson.FirstName + " " + profPerson.LastName,
                fechaHora, motivo, tipo, token);

            await _email.SendAsync(pacientePerson.Email, subject, body);
        }
        catch
        {
            // Email failures never abort the main operation
        }
    }

    private enum TurnoEmailTipo { Registrado, Confirmado, Cancelado, Recordatorio }

    private (string Subject, string Body) BuildTurnoEmail(
        string pacienteNombre, string profesionalNombre,
        DateTime fechaHora, string? motivo,
        TurnoEmailTipo tipo, string? token)
    {
        var localFecha = fechaHora;
        var fechaStr   = localFecha.ToString("dddd d 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-AR"));
        var horaStr    = localFecha.ToString("HH:mm");

        var (title, subtitle, ctaHtml, headerColor) = tipo switch
        {
            TurnoEmailTipo.Registrado   => (
                "Tu turno fue registrado",
                "Hemos registrado tu turno en SIGA Óptica. Por favor confirmalo haciendo clic en el botón:",
                token is not null ? $"""
                    <a href="{_frontendUrl}/confirmar-turno?token={token}"
                       style="display:inline-block;background:#00288E;color:#ffffff;text-decoration:none;
                              padding:14px 32px;border-radius:9999px;font-weight:700;font-size:15px;">
                      Confirmar mi turno
                    </a>
                    """ : "",
                "#00288E"),
            TurnoEmailTipo.Confirmado   => (
                "Tu turno fue confirmado ✓",
                "Tu cita ha sido confirmada. Te esperamos en SIGA Óptica.",
                "", "#065F46"),
            TurnoEmailTipo.Cancelado    => (
                "Tu turno fue cancelado",
                "Tu cita en SIGA Óptica fue cancelada. Si tenés dudas comunicate con nosotros.",
                "", "#991B1B"),
            TurnoEmailTipo.Recordatorio => (
                "Recordatorio: tu turno es mañana",
                "Te recordamos que tenés un turno agendado mañana en SIGA Óptica.",
                "", "#92400E"),
            _ => ("SIGA Óptica", "", "", "#00288E"),
        };

        var motivoRow = !string.IsNullOrWhiteSpace(motivo)
            ? $"""<tr><td style="color:#6B7280;padding:4px 0;width:120px">Motivo</td><td style="color:#111827;font-weight:600">{motivo}</td></tr>"""
            : "";

        var subject = tipo switch
        {
            TurnoEmailTipo.Registrado   => "Turno registrado — SIGA Óptica",
            TurnoEmailTipo.Confirmado   => "Turno confirmado — SIGA Óptica",
            TurnoEmailTipo.Cancelado    => "Turno cancelado — SIGA Óptica",
            TurnoEmailTipo.Recordatorio => "Recordatorio de turno — SIGA Óptica",
            _ => "SIGA Óptica",
        };

        var body = $"""
            <!DOCTYPE html>
            <html lang="es">
            <head><meta charset="UTF-8"><meta name="viewport" content="width=device-width,initial-scale=1"></head>
            <body style="margin:0;padding:0;background-color:#F7F9FE;font-family:Arial,sans-serif;">
              <table width="100%" cellpadding="0" cellspacing="0">
                <tr>
                  <td align="center" style="padding:40px 20px;">
                    <table width="560" cellpadding="0" cellspacing="0"
                      style="background:#ffffff;border-radius:16px;overflow:hidden;box-shadow:0 4px 24px rgba(0,0,0,0.06);">
                      <tr>
                        <td style="background:{headerColor};padding:28px 40px;">
                          <p style="margin:0;color:#ffffff;font-size:22px;font-weight:900;">SIGA Óptica</p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:40px;">
                          <h2 style="margin:0 0 8px;color:#181C20;font-size:22px;">{title}</h2>
                          <p style="color:#444653;margin:0 0 28px;">{subtitle}</p>

                          <table cellpadding="0" cellspacing="0"
                            style="width:100%;background:#F7F9FE;border-radius:12px;padding:20px;margin-bottom:28px;">
                            <tr>
                              <td colspan="2" style="padding-bottom:12px;font-size:11px;font-weight:700;
                                  letter-spacing:.08em;text-transform:uppercase;color:#9CA3AF;">
                                Datos del turno
                              </td>
                            </tr>
                            <tr>
                              <td style="color:#6B7280;padding:4px 0;width:120px">Paciente</td>
                              <td style="color:#111827;font-weight:600">{pacienteNombre}</td>
                            </tr>
                            <tr>
                              <td style="color:#6B7280;padding:4px 0">Profesional</td>
                              <td style="color:#111827;font-weight:600">{profesionalNombre}</td>
                            </tr>
                            <tr>
                              <td style="color:#6B7280;padding:4px 0">Fecha</td>
                              <td style="color:#111827;font-weight:600">{fechaStr}</td>
                            </tr>
                            <tr>
                              <td style="color:#6B7280;padding:4px 0">Hora</td>
                              <td style="color:#111827;font-weight:600">{horaStr} hs</td>
                            </tr>
                            {motivoRow}
                          </table>

                          {ctaHtml}

                          <p style="color:#757684;font-size:12px;margin:32px 0 0;">
                            Si no esperabas este correo o tenés dudas, contactate con nosotros.
                          </p>
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;

        return (subject, body);
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
        Id                   = t.Id,
        ProfessionalId       = t.ProfessionalId,
        ProfessionalNombre   = $"{t.Professional.User.Person.FirstName} {t.Professional.User.Person.LastName}",
        PatientId            = t.PatientId,
        PatientNombre        = $"{t.Patient.Person.FirstName} {t.Patient.Person.LastName}",
        FechaHora            = t.FechaHora,
        Estado               = t.Estado.ToString(),
        SolicitudCancelacion = t.SolicitudCancelacion,
        Motivo               = t.Motivo,
        Notas                = t.Notas,
        CreatedAt            = t.CreatedAt,
        UpdatedAt            = t.UpdatedAt,
    };
}
