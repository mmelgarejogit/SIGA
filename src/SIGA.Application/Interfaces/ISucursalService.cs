using SIGA.Application.Common;
using SIGA.Application.DTOs.Stock;

namespace SIGA.Application.Interfaces;

public interface ISucursalService
{
    Task<Result<IEnumerable<SucursalResponse>>> GetAllAsync(bool? isActive);
    Task<Result<SucursalResponse>> GetByIdAsync(Guid id);
    Task<Result<SucursalResponse>> CreateAsync(CreateSucursalRequest request);
    Task<Result<SucursalResponse>> UpdateAsync(Guid id, UpdateSucursalRequest request);
    Task<Result<bool>> DeactivateAsync(Guid id);
}
