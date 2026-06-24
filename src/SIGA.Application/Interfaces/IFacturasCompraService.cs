using SIGA.Application.Common;
using SIGA.Application.DTOs.Compras;

namespace SIGA.Application.Interfaces;

public interface IFacturasCompraService
{
    Task<Result<PagedResult<FacturaCompraResponse>>> GetFacturasAsync(
        int? proveedorId,
        string? condicionVenta,
        string? estado,
        string? origen,
        string? fechaDesde,
        string? fechaHasta,
        string? search,
        int page,
        int pageSize);

    Task<Result<FacturaCompraResponse>> GetFacturaByIdAsync(int id);

    Task<Result<FacturaCompraResponse>> RegistrarFacturaDirectaAsync(RegistrarFacturaDirectaRequest request);

    Task<Result<FacturaCompraResponse>> AnularFacturaAsync(int id, AnularFacturaRequest request);
}
