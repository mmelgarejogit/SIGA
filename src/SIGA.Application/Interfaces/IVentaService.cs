using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;

namespace SIGA.Application.Interfaces;

public interface IVentaService
{
    Task<Result<VentaDto>> GetVentaByIdAsync(int id);
    Task<Result<PagedResult<VentaDto>>> GetVentasAsync(
        string? estado, string? tipo, string? fechaDesde, string? fechaHasta,
        int? patientId, int page, int pageSize);

    Task<Result<VentaDto>> CrearVentaAsync(CrearVentaRequest request);
    Task<Result<VentaDto>> ConfirmarVentaAsync(int id, int userId);
    Task<Result<VentaDto>> CancelarVentaAsync(int id, CancelarVentaRequest request);
    Task<Result<bool>> EliminarPresupuestoAsync(int id);
    Task<Result<VentaDto>> RegistrarCobroAsync(RegistrarCobroRequest request, int userId);
    Task<Result<VentaDto>> EmitirComprobanteAsync(int ventaId, int userId);
    Task<Result<VentaDto>> EmitirFacturaAsync(EmitirFacturaRequest request);
    Task<Result<List<VentaDto>>> GetCobrosPendientesAsync();

    // Trabajo a pedido — desde venta
    Task<Result<VentaDto>> CrearTrabajoPedidoAsync(int ventaId, CrearTrabajoPedidoRequest request);

    // Trabajo a pedido — vistas globales
    Task<Result<List<TrabajoPedidoListDto>>> GetTrabajosPedidoAsync(string? estado);
    Task<Result<TrabajoPedidoListDto>> GestionarAprobacionAsync(int id, GestionarTrabajoPedidoRequest request, int userId, string userName);
    Task<Result<TrabajoPedidoListDto>> RegistrarEnvioLabAsync(int id);
    Task<Result<TrabajoPedidoListDto>> RegistrarRecepcionLabAsync(int id);
    Task<Result<TrabajoPedidoListDto>> EmitirFacturaLaboratorioAsync(int id, EmitirFacturaLaboratorioRequest request, int userId);

    // Devoluciones
    Task<Result<DevolucionDto>> SolicitarDevolucionAsync(int ventaId, SolicitarDevolucionRequest request, int userId, string userName);
    Task<Result<List<DevolucionDto>>> GetDevolucionesAsync(int ventaId);
    Task<Result<DevolucionDto>> GestionarDevolucionAsync(int devolucionId, GestionarDevolucionRequest request, int userId, string userName);
}
