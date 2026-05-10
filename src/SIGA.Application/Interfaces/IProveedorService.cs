using SIGA.Application.Common;
using SIGA.Application.DTOs.Inventario;

namespace SIGA.Application.Interfaces;

public interface IProveedorService
{
    Task<Result<IEnumerable<ProveedorResponse>>> GetAllAsync(string? search);
    Task<Result<ProveedorResponse>> CreateAsync(CreateProveedorRequest request);
    Task<Result<ProveedorResponse>> UpdateAsync(int id, CreateProveedorRequest request);
    Task<Result<bool>> DeactivateAsync(int id);
    Task<Result<PedidoProveedorResponse>> CreatePedidoAsync(CreatePedidoProveedorRequest request);
    Task<Result<IEnumerable<PedidoProveedorResponse>>> GetPedidosAsync(int? proveedorId, string? estado);
    Task<Result<PedidoProveedorResponse>> UpdatePedidoEstadoAsync(int id, string estado);
    Task<Result<bool>> CancelPedidoAsync(int id);
}
