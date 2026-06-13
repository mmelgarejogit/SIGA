using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;

namespace SIGA.Application.Interfaces;

public interface IServicioService
{
    Task<Result<IEnumerable<ServicioDto>>> GetAllAsync();
    Task<Result<ServicioDto>> CreateAsync(CreateServicioRequest request);
    Task<Result<ServicioDto>> UpdateAsync(int id, UpdateServicioRequest request);
    Task<Result<bool>> DeactivateAsync(int id);

    Task<Result<ServicioDto>> AddTarifaAsync(int servicioId, CreateServicioTarifaRequest request);
    Task<Result<bool>> RemoveTarifaAsync(int tarifaId);
    Task<Result<PrecioResueltoDto>> ResolvePrecioAsync(int servicioId, int? professionalId);
}
