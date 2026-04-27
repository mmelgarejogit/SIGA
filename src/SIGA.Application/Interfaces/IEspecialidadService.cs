using SIGA.Application.Common;
using SIGA.Application.DTOs.Especialidades;

namespace SIGA.Application.Interfaces;

public interface IEspecialidadService
{
    Task<Result<IEnumerable<EspecialidadResponse>>> GetAllAsync();
    Task<Result<EspecialidadResponse>> GetByIdAsync(int id);
    Task<Result<EspecialidadResponse>> CreateAsync(CreateEspecialidadRequest request);
    Task<Result<EspecialidadResponse>> UpdateAsync(int id, UpdateEspecialidadRequest request);
    Task<Result<bool>> DeleteAsync(int id);
}
