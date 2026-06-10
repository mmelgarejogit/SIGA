using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;

namespace SIGA.Application.Interfaces;

public interface ITimbradoService
{
    Task<Result<IEnumerable<TimbradoDto>>> GetAllAsync();
    Task<Result<IEnumerable<TimbradoDto>>> GetActivosAsync();
    Task<Result<TimbradoDto>> GetByIdAsync(int id);
    Task<Result<TimbradoDto>> CreateAsync(CreateTimbradoRequest request);
    Task<Result<TimbradoDto>> UpdateAsync(int id, UpdateTimbradoRequest request);
    Task<Result<bool>> DeactivateAsync(int id);
}