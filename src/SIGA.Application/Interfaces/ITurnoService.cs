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
}
