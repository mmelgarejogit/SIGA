using SIGA.Application.Common;
using SIGA.Application.DTOs.Compras;

namespace SIGA.Application.Interfaces;

public interface IRecepcionesService
{
    Task<Result<PagedResult<RecepcionListResponse>>> GetRecepcionesAsync(
        int? proveedorId,
        string? estadoOC,
        string? fechaDesde,
        string? fechaHasta,
        int page,
        int pageSize);

    Task<Result<List<FacturaDisponibleResponse>>> GetFacturasDisponiblesAsync();

    Task<Result<RecepcionListResponse>> RegistrarRecepcionAsync(int userId, RegistrarRecepcionRequest request);

    Task<Result<RecepcionDetalleResponse>> GetByIdAsync(int id);
}
