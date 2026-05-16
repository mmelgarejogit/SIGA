using SIGA.Application.Common;
using SIGA.Application.DTOs.Estados;

namespace SIGA.Application.Interfaces;

public interface IEstadoConfigService
{
    Task<Result<IEnumerable<EstadoConfigResponse>>> GetByEntidadAsync(string? entidad);
    Task<Result<EstadoConfigResponse>> CreateAsync(CreateEstadoConfigRequest request);
    Task<Result<EstadoConfigResponse>> UpdateAsync(int id, UpdateEstadoConfigRequest request);
    Task<Result<bool>> DeleteAsync(int id);
}
