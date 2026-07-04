using SIGA.Application.Common;
using SIGA.Application.DTOs.Sucursales;

namespace SIGA.Application.Interfaces;

public interface ISucursalService
{
    Task<Result<IEnumerable<SucursalResponse>>> GetAllAsync(bool soloActivas = false);
    Task<Result<SucursalResponse>> GetByIdAsync(int id);
    Task<Result<SucursalResponse>> CreateAsync(CreateSucursalRequest request);
    Task<Result<SucursalResponse>> UpdateAsync(int id, UpdateSucursalRequest request);
    /// <summary>Borrado lógico: marca IsActive = false.</summary>
    Task<Result<bool>> DeleteAsync(int id);
}
