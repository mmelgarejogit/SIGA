using SIGA.Application.Common;
using SIGA.Application.DTOs.Turnos;

namespace SIGA.Application.Interfaces;

public interface ITurnoService
{
    Task<Result<IEnumerable<TurnoResponse>>> GetAllAsync(DateOnly? fecha, int? professionalId, string? estado);
    Task<Result<IEnumerable<SlotDisponibleResponse>>> GetSlotsDisponiblesAsync(int professionalId, DateOnly fecha);
    Task<Result<TurnoResponse>> CreateAsync(CreateTurnoRequest request);
    Task<Result<TurnoResponse>> UpdateEstadoAsync(int id, UpdateTurnoEstadoRequest request);
    Task<Result<bool>> CancelAsync(int id);

    // ── Patient self-scheduling ──
    Task<Result<IEnumerable<TurnoResponse>>> GetMisTurnosAsync(int userId);
    Task<Result<TurnoResponse>> SelfBookAsync(int userId, SelfBookTurnoRequest request);
    Task<Result<bool>> SolicitarCancelacionAsync(int turnoId, int userId);
    Task<Result<bool>> GestionarCancelacionAsync(int turnoId, GestionarCancelacionRequest request);
}
