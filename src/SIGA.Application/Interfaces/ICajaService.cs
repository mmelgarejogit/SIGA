using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;

namespace SIGA.Application.Interfaces;

public interface ICajaService
{
    Task<Result<ResumenCajaDto>> GetResumenAsync(string fecha);
    Task<Result<PagedResult<MovimientoCajaDto>>> GetMovimientosAsync(
        string? fechaDesde, string? fechaHasta, string? tipo, int page, int pageSize);

    // Sesiones de caja
    Task<Result<SesionCajaDto?>> GetSesionActualAsync();
    Task<Result<decimal>> GetMontoAperturaSugeridoAsync();
    Task<Result<SesionCajaDto>> AbrirSesionAsync(AbrirSesionRequest request, int userId);
    Task<Result<SesionCajaDto>> GetSesionByIdAsync(int id);
    Task<Result<SesionCajaDto>> CerrarSesionAsync(int id, CerrarSesionRequest request, int userId);
    Task<Result<PagedResult<SesionCajaListDto>>> GetSesionesAsync(int page, int pageSize, string? estado = null);

    // Aprobación de cierre
    Task<Result<SesionCajaDto>> AprobarCierreAsync(int id, int userId);
    Task<Result<SesionCajaDto>> RechazarCierreAsync(int id, RechazarCierreRequest request, int userId);
}
