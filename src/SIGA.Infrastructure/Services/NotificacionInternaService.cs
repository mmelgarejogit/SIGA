using Microsoft.EntityFrameworkCore;
using SIGA.Application.Common;
using SIGA.Application.DTOs.Notificaciones;
using SIGA.Application.Interfaces;
using SIGA.Domain.Entities;
using SIGA.Infrastructure.Persistence;

namespace SIGA.Infrastructure.Services;

public class NotificacionInternaService(AppDbContext db, ICurrentUserContext current) : INotificacionInternaService
{
    public async Task CrearAsync(
        string tipo, string mensaje,
        string? entidadOrigenTipo = null, int? entidadOrigenId = null,
        int? destinatarioUsuarioId = null, int? destinatarioSucursalId = null)
    {
        db.NotificacionesInternas.Add(new NotificacionInterna
        {
            Tipo                    = tipo,
            Mensaje                 = mensaje,
            EntidadOrigenTipo       = entidadOrigenTipo,
            EntidadOrigenId         = entidadOrigenId,
            DestinatarioUsuarioId   = destinatarioUsuarioId,
            DestinatarioSucursalId  = destinatarioSucursalId,
            FechaCreacion           = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
    }

    public async Task<Result<PagedResult<NotificacionDto>>> GetMisNotificacionesAsync(bool? soloNoLeidas, int page, int pageSize)
    {
        var query = VisibleQuery();

        if (soloNoLeidas == true)
            query = query.Where(n => !n.Leido);

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(n => n.FechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ToDtoExpression)
            .ToListAsync();

        return Result<PagedResult<NotificacionDto>>.Success(new PagedResult<NotificacionDto>
        {
            Items      = items,
            TotalCount = totalCount,
            Page       = page,
            PageSize   = pageSize,
            TotalPages = totalPages,
        });
    }

    public async Task<Result<int>> GetContadorNoLeidasAsync()
    {
        var count = await VisibleQuery().CountAsync(n => !n.Leido);
        return Result<int>.Success(count);
    }

    public async Task<Result<bool>> MarcarLeidaAsync(int id)
    {
        var notificacion = await VisibleQuery().FirstOrDefaultAsync(n => n.Id == id);
        if (notificacion is null)
            return Result<bool>.Failure("Notificación no encontrada.", ErrorType.NotFound);

        if (!notificacion.Leido)
        {
            notificacion.Leido        = true;
            notificacion.FechaLectura = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> MarcarTodasLeidasAsync()
    {
        var now = DateTime.UtcNow;
        await VisibleQuery()
            .Where(n => !n.Leido)
            .ExecuteUpdateAsync(s => s
                .SetProperty(n => n.Leido, true)
                .SetProperty(n => n.FechaLectura, now));

        return Result<bool>.Success(true);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private IQueryable<NotificacionInterna> VisibleQuery()
    {
        var userId = current.UserId;
        var sucursalId = current.SucursalId;

        // current.EsGlobal (SucursalId == null) también es true para pacientes, que no
        // tienen sucursal asignada por diseño — no sirve para distinguir "staff sin
        // sucursal fija" (admin) de "no es staff" (paciente). Los broadcasts (globales o
        // por sucursal) solo son visibles para quienes son staff: tienen una sucursal
        // asignada, o tienen visión cross-sucursal real (ver_todas_sucursales, admin).
        // Cualquier otra cuenta (pacientes) solo ve notificaciones dirigidas a su usuario.
        var veTodasLasSucursales = current.TienePermiso("ver_todas_sucursales");
        var esStaff = sucursalId != null || veTodasLasSucursales;

        return db.NotificacionesInternas.Where(n =>
            n.DestinatarioUsuarioId == userId ||
            (esStaff && n.DestinatarioUsuarioId == null && n.DestinatarioSucursalId == null) ||
            (esStaff && n.DestinatarioUsuarioId == null && n.DestinatarioSucursalId != null &&
                (veTodasLasSucursales || n.DestinatarioSucursalId == sucursalId)));
    }

    private static readonly System.Linq.Expressions.Expression<Func<NotificacionInterna, NotificacionDto>> ToDtoExpression = n => new NotificacionDto
    {
        Id                = n.Id,
        Tipo              = n.Tipo,
        Mensaje           = n.Mensaje,
        EntidadOrigenTipo = n.EntidadOrigenTipo,
        EntidadOrigenId   = n.EntidadOrigenId,
        Leido             = n.Leido,
        FechaCreacion     = n.FechaCreacion,
        FechaLectura      = n.FechaLectura,
    };
}
