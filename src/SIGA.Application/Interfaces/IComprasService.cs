using SIGA.Application.Common;
using SIGA.Application.DTOs.Compras;

namespace SIGA.Application.Interfaces;

public interface IComprasService
{
    Task<Result<PedidoResponse>> CrearPedidoAsync(CrearPedidoRequest request);
    Task<Result<PedidoResponse>> EditarPedidoAsync(int id, CrearPedidoRequest request);
    Task<Result<PedidoResponse>> ConfirmarPedidoAsync(int id);
    Task<Result<PedidoResponse>> RegistrarFacturaAsync(int id, RegistrarFacturaPedidoRequest request);
    Task<Result<DevolucionResponse>> RegistrarDevolucionAsync(int id, RegistrarDevolucionRequest request);
    Task<Result<PagedResult<PedidoResponse>>> GetPedidosAsync(int? proveedorId, string? estado, int page, int pageSize);
    Task<Result<PedidoResponse>> GetPedidoByIdAsync(int id);
    Task<Result<PedidoResponse>> CancelarPedidoAsync(int id);
}
