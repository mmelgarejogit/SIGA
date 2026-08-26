using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Notificaciones;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class NotificacionPreferenciaService(AppDbContext db, ICurrentUserContext current) : INotificacionPreferenciaService
{
    public async Task<Result<NotificacionPreferenciaResponse>> GetPropiaAsync()
    {
        var personId = await db.Users
            .Where(u => u.Id == current.UserId)
            .Select(u => (int?)u.PersonId)
            .FirstOrDefaultAsync();

        if (personId is null)
            return Result<NotificacionPreferenciaResponse>.Failure("Usuario no encontrado.", ErrorType.NotFound);

        return await GetByPersonaAsync(personId.Value);
    }

    public async Task<Result<NotificacionPreferenciaResponse>> UpdatePropiaAsync(UpdateNotificacionPreferenciaRequest request)
    {
        var personId = await db.Users
            .Where(u => u.Id == current.UserId)
            .Select(u => (int?)u.PersonId)
            .FirstOrDefaultAsync();

        if (personId is null)
            return Result<NotificacionPreferenciaResponse>.Failure("Usuario no encontrado.", ErrorType.NotFound);

        return await UpdateByPersonaAsync(personId.Value, request);
    }

    public async Task<Result<NotificacionPreferenciaResponse>> GetByPersonaAsync(int personId)
    {
        if (!await db.Persons.AnyAsync(p => p.Id == personId))
            return Result<NotificacionPreferenciaResponse>.Failure("Persona no encontrada.", ErrorType.NotFound);

        var pref = await db.NotificacionesPreferencias.FirstOrDefaultAsync(p => p.PersonId == personId);

        return Result<NotificacionPreferenciaResponse>.Success(pref is null ? Defaults(personId) : ToResponse(pref));
    }

    public async Task<Result<NotificacionPreferenciaResponse>> UpdateByPersonaAsync(int personId, UpdateNotificacionPreferenciaRequest request)
    {
        if (!await db.Persons.AnyAsync(p => p.Id == personId))
            return Result<NotificacionPreferenciaResponse>.Failure("Persona no encontrada.", ErrorType.NotFound);

        if ((request.VentanaSilencioInicio is null) != (request.VentanaSilencioFin is null))
            return Result<NotificacionPreferenciaResponse>.Failure(
                "La ventana de silencio necesita hora de inicio y de fin, o ninguna de las dos.", ErrorType.Validation);

        var now  = DateTime.UtcNow;
        var pref = await db.NotificacionesPreferencias.FirstOrDefaultAsync(p => p.PersonId == personId);

        if (pref is null)
        {
            pref = new NotificacionPreferencia { PersonId = personId, CreatedAt = now };
            db.NotificacionesPreferencias.Add(pref);
        }

        pref.RecibirEmail            = request.RecibirEmail;
        pref.VentanaSilencioInicio   = request.VentanaSilencioInicio;
        pref.VentanaSilencioFin      = request.VentanaSilencioFin;
        pref.UpdatedAt               = now;

        await db.SaveChangesAsync();

        return Result<NotificacionPreferenciaResponse>.Success(ToResponse(pref));
    }

    private static NotificacionPreferenciaResponse Defaults(int personId) => new()
    {
        PersonId              = personId,
        RecibirEmail          = true,
        VentanaSilencioInicio = null,
        VentanaSilencioFin    = null,
        UpdatedAt             = null,
    };

    private static NotificacionPreferenciaResponse ToResponse(NotificacionPreferencia p) => new()
    {
        PersonId              = p.PersonId,
        RecibirEmail          = p.RecibirEmail,
        VentanaSilencioInicio = p.VentanaSilencioInicio,
        VentanaSilencioFin    = p.VentanaSilencioFin,
        UpdatedAt             = p.UpdatedAt,
    };
}
