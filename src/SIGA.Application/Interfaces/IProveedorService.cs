using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;

namespace SIGA.Application.Interfaces;

public interface IProveedorService
{
    Task<Result<IEnumerable<ProveedorResponse>>> GetAllAsync(string? search);
    Task<Result<ProveedorResponse>> CreateAsync(CreateProveedorRequest request);
    Task<Result<ProveedorResponse>> UpdateAsync(int id, CreateProveedorRequest request);
    Task<Result<bool>> DeactivateAsync(int id);
}
