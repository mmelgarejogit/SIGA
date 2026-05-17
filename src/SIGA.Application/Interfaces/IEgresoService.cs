using SIGA.Application.Common;
using SIGA.Application.DTOs.Egresos;

namespace SIGA.Application.Interfaces;

public interface IEgresoService
{
    Task<Result<EgresoResponse>> CrearFacturaCompraAsync(CrearFacturaCompraRequest request);
    Task<Result<EgresoResponse>> CrearHonorarioAsync(CrearHonorarioRequest request);
    Task<Result<EgresoResponse>> CrearGastoGeneralAsync(CrearGastoGeneralRequest request);
    Task<Result<EgresoResponse>> RegistrarPagoAsync(int id, RegistrarPagoRequest request);
    Task<Result<EgresoResponse>> AnularEgresoAsync(int id, AnularEgresoRequest request);
    Task<Result<EgresoResponse>> GetEgresoByIdAsync(int id);
    Task<Result<PagedResult<EgresoResponse>>> GetEgresosAsync(
        string? tipo, string? estado, string? fechaDesde, string? fechaHasta,
        bool? soloVencidos, int page, int pageSize);
    Task<Result<IEnumerable<CategoriaGastoResponse>>> GetCategoriasAsync();
    Task<Result<CategoriaGastoResponse>> CrearCategoriaAsync(CrearCategoriaGastoRequest request);
    Task<Result<CategoriaGastoResponse>> ActualizarCategoriaAsync(int id, ActualizarCategoriaGastoRequest request);
}
