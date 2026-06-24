using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;

namespace SIGA.Application.Interfaces;

public interface ITratamientoService
{
    Task<Result<IEnumerable<TratamientoDto>>> GetAllAsync();
    Task<Result<TratamientoDto>> CreateAsync(CreateTratamientoRequest request);
    Task<Result<TratamientoDto>> UpdateAsync(int id, UpdateTratamientoRequest request);
    Task<Result<bool>> DeactivateAsync(int id);
}
