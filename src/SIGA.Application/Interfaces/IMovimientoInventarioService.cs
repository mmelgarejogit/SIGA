using SIGA.Application.Common;
using SIGA.Application.DTOs.Stock;

namespace SIGA.Application.Interfaces;

public interface IMovimientoInventarioService
{
    Task<Result<PagedResult<MovimientoInventarioResponse>>> GetAllAsync(
        int page, int pageSize, Guid? sucursalId, Guid? productoVarianteId,
        string? tipo, string? origen);
    Task<Result<IEnumerable<StockPorVarianteResponse>>> GetStockAsync(
        Guid? sucursalId, Guid? productoVarianteId, bool? bajoStock);
    Task<Result<int>> GetStockActualAsync(Guid productoVarianteId, Guid sucursalId);
}
