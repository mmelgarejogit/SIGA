using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;

namespace SIGA.Application.Interfaces;

public interface IProductoService
{
    Task<Result<PagedResult<ProductoResponse>>> GetAllAsync(int page, int pageSize, string? search, string? categoria, bool? bajoStock);
    Task<Result<ProductoResponse>> GetByIdAsync(int id);
    Task<Result<ProductoResponse>> CreateAsync(CreateProductoRequest request);
    Task<Result<ProductoResponse>> UpdateAsync(int id, UpdateProductoRequest request);
    Task<Result<bool>> DeactivateAsync(int id);
    Task<Result<MovimientoStockResponse>> RegistrarMovimientoAsync(int productoId, CreateMovimientoStockRequest request);
    Task<Result<IEnumerable<MovimientoStockResponse>>> GetMovimientosAsync(int productoId);
    Task<Result<PagedResult<MovimientoStockResponse>>> GetAllMovimientosAsync(int page, int pageSize, string? tipo);
}
