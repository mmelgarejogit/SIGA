using SIGA.Application.Common;
using SIGA.Application.DTOs.Notificaciones;

namespace SIGA.Application.Interfaces;

public interface INotificacionPreferenciaService
{
    Task<Result<NotificacionPreferenciaResponse>> GetPropiaAsync();
    Task<Result<NotificacionPreferenciaResponse>> UpdatePropiaAsync(UpdateNotificacionPreferenciaRequest request);

    Task<Result<NotificacionPreferenciaResponse>> GetByPersonaAsync(int personId);
    Task<Result<NotificacionPreferenciaResponse>> UpdateByPersonaAsync(int personId, UpdateNotificacionPreferenciaRequest request);
}
