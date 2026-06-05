using SIGA.Application.Common;
using SIGA.Application.DTOs.Ubicacion;

namespace SIGA.Application.Interfaces;

public interface IUbicacionService
{
    Task<Result<IEnumerable<DepartamentoResponse>>> GetDepartamentosAsync(bool? isActive);
    Task<Result<DepartamentoResponse>> CreateDepartamentoAsync(CreateDepartamentoRequest request);
    Task<Result<DepartamentoResponse>> UpdateDepartamentoAsync(int id, UpdateDepartamentoRequest request);

    Task<Result<IEnumerable<CiudadResponse>>> GetCiudadesAsync(int? departamentoId, bool? isActive);
    Task<Result<CiudadResponse>> CreateCiudadAsync(CreateCiudadRequest request);
    Task<Result<CiudadResponse>> UpdateCiudadAsync(int id, UpdateCiudadRequest request);
}
