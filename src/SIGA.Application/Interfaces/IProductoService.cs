using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;

namespace SIGA.Application.Interfaces;

public interface IProductoService
{
    Task<Result<PagedResult<ProductoResponse>>> GetAllAsync(int page, int pageSize, string? search, string? categoria, bool? bajoStock, string? tipoCategoria);
    Task<Result<ProductoResponse>> GetByIdAsync(int id);
    Task<Result<ProductoResponse>> CreateAsync(CreateProductoRequest request);
    Task<Result<ProductoResponse>> UpdateAsync(int id, UpdateProductoRequest request);
    Task<Result<bool>> DeactivateAsync(int id);
    Task<Result<bool>> DeleteAsync(int id);
    Task<Result<MovimientoStockResponse>> RegistrarMovimientoAsync(int productoId, CreateMovimientoStockRequest request);
    Task<Result<MovimientoStockResponse>> AprobarRechazarMovimientoAsync(int id, AprobarRechazarMovimientoRequest request);
    Task<Result<MovimientoStockResponse>> GetMovimientoByIdAsync(int id);
    Task<Result<IEnumerable<MovimientoStockResponse>>> GetMovimientosAsync(int productoId);
    Task<Result<PagedResult<MovimientoStockResponse>>> GetAllMovimientosAsync(int page, int pageSize, string? tipo, string? estado);

    Task<Result<ProductoResponse>> UpdateStockConfigAsync(int id, UpdateStockConfigRequest request);
    Task<Result<string>> UploadImagenAsync(int id, Stream stream, string fileName);
    Task<Result<bool>> DeleteImagenAsync(int id);

    Task<Result<IEnumerable<CategoriaProductoResponse>>> GetCategoriasAsync();
    Task<Result<CategoriaProductoResponse>> CreateCategoriaAsync(CreateCategoriaProductoRequest request);
    Task<Result<CategoriaProductoResponse>> UpdateCategoriaAsync(int id, UpdateCategoriaProductoRequest request);
    Task<Result<bool>> DeactivateCategoriaAsync(int id);
    Task<Result<bool>> DeleteCategoriaAsync(int id);
}
