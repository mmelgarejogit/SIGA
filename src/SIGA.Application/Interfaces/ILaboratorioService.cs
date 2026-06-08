using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;

namespace SIGA.Application.Interfaces;

public interface ILaboratorioService
{
    Task<Result<List<TrabajoPedidoListDto>>> GetPedidosAsync(string? estado);
    Task<Result<TrabajoPedidoListDto>> CrearPedidoAsync(CrearTrabajoPedidoRequest request);
    Task<Result<TrabajoPedidoListDto>> GestionarAprobacionAsync(int id, GestionarTrabajoPedidoRequest request, int userId, string userName);
    Task<Result<TrabajoPedidoListDto>> RegistrarEnvioAsync(int id);
    Task<Result<TrabajoPedidoListDto>> RegistrarRecepcionAsync(int id);
    Task<Result<TrabajoPedidoListDto>> EmitirFacturaAsync(int id, EmitirFacturaLaboratorioRequest request, int userId);
}
