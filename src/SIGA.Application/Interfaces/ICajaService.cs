using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;

namespace SIGA.Application.Interfaces;

public interface ICajaService
{
    Task<Result<ResumenCajaDto>> GetResumenAsync(string fecha);
    Task<Result<PagedResult<MovimientoCajaDto>>> GetMovimientosAsync(
        string? fechaDesde, string? fechaHasta, string? tipo, int page, int pageSize);
}
