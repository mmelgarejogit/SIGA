using SIGA.Application.Common;
using SIGA.Application.DTOs.Notificaciones;
using SIGA.Domain.Entities;

namespace SIGA.Application.Interfaces;

public interface INotificacionInternaService
{
    /// <summary>Helper interno para que otros servicios disparen notificaciones. Ambos destinatarios null = global.</summary>
    Task CrearAsync(
        TipoNotificacion tipo, string mensaje,
        string? entidadOrigenTipo = null, int? entidadOrigenId = null,
        int? destinatarioUsuarioId = null, int? destinatarioSucursalId = null);

    Task<Result<PagedResult<NotificacionDto>>> GetMisNotificacionesAsync(bool? soloNoLeidas, int page, int pageSize);
    Task<Result<int>> GetContadorNoLeidasAsync();
    Task<Result<bool>> MarcarLeidaAsync(int id);
    Task<Result<bool>> MarcarTodasLeidasAsync();
}
