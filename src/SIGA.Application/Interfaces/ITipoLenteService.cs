using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;

namespace SIGA.Application.Interfaces;

public interface ITipoLenteService
{
    Task<Result<IEnumerable<TipoLenteDto>>> GetAllAsync();
    Task<Result<TipoLenteDto>> CreateAsync(CreateTipoLenteRequest request);
    Task<Result<TipoLenteDto>> UpdateAsync(int id, UpdateTipoLenteRequest request);
    Task<Result<bool>> DeactivateAsync(int id);
    Task<Result<bool>> DeleteAsync(int id);
}
