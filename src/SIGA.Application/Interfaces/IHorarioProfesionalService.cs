using SIGA.Application.Common;
using SIGA.Application.DTOs.Horarios;

namespace SIGA.Application.Interfaces;

public interface IHorarioProfesionalService
{
    Task<Result<IEnumerable<HorarioProfesionalResponse>>> GetHorariosAsync(int professionalId);
    Task<Result<IEnumerable<HorarioProfesionalResponse>>> SetHorariosAsync(int professionalId, SetHorariosRequest request);
    Task<Result<IEnumerable<BloqueoFechaResponse>>> GetBloqueosAsync(int professionalId);
    Task<Result<BloqueoFechaResponse>> AddBloqueoAsync(int professionalId, BloqueoFechaRequest request);
    Task<Result<bool>> RemoveBloqueoAsync(int professionalId, int bloqueoId);
}
