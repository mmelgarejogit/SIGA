using SIGA.Application.Common;
using SIGA.Application.DTOs.Stock;

namespace SIGA.Application.Interfaces;

public interface IParametroStockService
{
    Task<Result<IEnumerable<ParametroStockResponse>>> GetAllAsync(Guid? sucursalId, Guid? productoVarianteId);
    Task<Result<ParametroStockResponse>> UpsertAsync(UpsertParametroStockRequest request);
    Task<Result<bool>> DeleteAsync(Guid productoVarianteId, Guid sucursalId);
}
