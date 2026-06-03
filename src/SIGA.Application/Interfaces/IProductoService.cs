using SIGA.Application.Common;
using SIGA.Application.DTOs.Productos;

namespace SIGA.Application.Interfaces;

public interface IProductoService
{
    Task<Result<PagedResult<ProductoResponse>>> GetAllAsync(int page, int pageSize, string? search, int? categoriaId, bool? isActive);
    Task<Result<ProductoResponse>> GetByIdAsync(int id);
    Task<Result<ProductoResponse>> CreateAsync(CreateProductoRequest request);
    Task<Result<ProductoResponse>> UpdateAsync(int id, UpdateProductoRequest request);
    Task<Result<bool>> DeactivateAsync(int id);

    Task<Result<IEnumerable<ProductoVarianteResponse>>> GetVariantesAsync(int productoId);
    Task<Result<ProductoVarianteResponse>> GetVarianteByIdAsync(Guid id);
    Task<Result<ProductoVarianteResponse>> CreateVarianteAsync(CreateProductoVarianteRequest request);
    Task<Result<ProductoVarianteResponse>> UpdateVarianteAsync(Guid id, UpdateProductoVarianteRequest request);
    Task<Result<bool>> DeactivateVarianteAsync(Guid id);
    Task<Result<string>> UploadVarianteImagenAsync(Guid id, Stream stream, string fileName);

    Task<Result<IEnumerable<CategoriaProductoResponse>>> GetCategoriasAsync();
    Task<Result<CategoriaProductoResponse>> CreateCategoriaAsync(CreateCategoriaProductoRequest request);
    Task<Result<CategoriaProductoResponse>> UpdateCategoriaAsync(int id, UpdateCategoriaProductoRequest request);
    Task<Result<bool>> DeactivateCategoriaAsync(int id);
}
