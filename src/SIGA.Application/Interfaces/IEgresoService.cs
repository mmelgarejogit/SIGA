using SIGA.Application.Common;
using SIGA.Application.DTOs.Egresos;

namespace SIGA.Application.Interfaces;

public interface IEgresoService
{
    Task<Result<EgresoResponse>> CrearFacturaCompraAsync(CrearFacturaCompraRequest request, int userId);
    Task<Result<EgresoResponse>> CrearHonorarioAsync(CrearHonorarioRequest request, int userId);
    Task<Result<EgresoResponse>> CrearGastoGeneralAsync(CrearGastoGeneralRequest request, int userId);
    Task<Result<EgresoResponse>> CrearSalarioAsync(CrearSalarioRequest request, int userId);
    Task<Result<EgresoResponse>> RegistrarPagoAsync(int id, RegistrarPagoRequest request, int userId);
    Task<Result<EgresoResponse>> AprobarEgresoAsync(int id, int userId);
    Task<Result<EgresoResponse>> RechazarEgresoAsync(int id, RechazarEgresoRequest request);
    Task<Result<EgresoResponse>> AnularEgresoAsync(int id, AnularEgresoRequest request);
    Task<Result<EgresoResponse>> GetEgresoByIdAsync(int id);
    Task<Result<PagedResult<EgresoResponse>>> GetEgresosAsync(
        string? tipo, string? estado, string? fechaDesde, string? fechaHasta,
        bool? soloVencidos, int page, int pageSize, Guid? sucursalId = null);
    Task<Result<IEnumerable<CategoriaGastoResponse>>> GetCategoriasAsync();
    Task<Result<CategoriaGastoResponse>> CrearCategoriaAsync(CrearCategoriaGastoRequest request);
    Task<Result<CategoriaGastoResponse>> ActualizarCategoriaAsync(int id, ActualizarCategoriaGastoRequest request);
}