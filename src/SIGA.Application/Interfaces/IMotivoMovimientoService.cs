using SIGA.Application.Common;
using SIGA.Application.DTOs.Productos;

namespace SIGA.Application.Interfaces;

public interface IMotivoMovimientoService
{
    Task<Result<IEnumerable<MotivoMovimientoResponse>>> GetAllAsync(string? tipo);
    Task<Result<MotivoMovimientoResponse>> CreateAsync(CreateMotivoMovimientoRequest request);
    Task<Result<MotivoMovimientoResponse>> UpdateAsync(int id, UpdateMotivoMovimientoRequest request);
    Task<Result<bool>> DeactivateAsync(int id);
}
