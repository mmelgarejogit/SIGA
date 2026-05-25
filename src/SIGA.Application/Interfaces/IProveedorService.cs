using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;

namespace SIGA.Application.Interfaces;

public interface IProveedorService
{
    Task<Result<PagedResult<ProveedorResponse>>> GetAllAsync(int page, int pageSize, string? search, bool? isActive);
    Task<Result<ProveedorResponse>> CreateAsync(CreateProveedorRequest request);
    Task<Result<ProveedorResponse>> UpdateAsync(int id, CreateProveedorRequest request);
    Task<Result<bool>> DeactivateAsync(int id);
}
