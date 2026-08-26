using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;

namespace SIGA.Application.Interfaces;

public interface ILaboratorioService
{
    Task<Result<List<TrabajoPedidoListDto>>> GetPedidosAsync(string? estado);
    Task<Result<TrabajoPedidoListDto>> GestionarAprobacionAsync(int id, GestionarTrabajoPedidoRequest request, int userId, string userName);
    Task<Result<TrabajoPedidoListDto>> RegistrarEnvioAsync(int id, RegistrarEnvioRequest request);
    Task<Result<TrabajoPedidoListDto>> RegistrarRecepcionAsync(int id);
    Task<Result<TrabajoPedidoListDto>> RegistrarEntregaAsync(int id, RegistrarEntregaRequest request, int userId);
    Task<Result<TrabajoPedidoListDto>> RegistrarRetrabajoAsync(int id, RegistrarRetrabajoRequest request, int userId);
    Task<Result<TrabajoPedidoListDto>> EmitirFacturaAsync(int id, EmitirFacturaLaboratorioRequest request, int userId);
}
