using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;

namespace SIGA.Application.Interfaces;

public interface IVentaService
{
    Task<Result<VentaDto>> GetVentaByIdAsync(int id);
    Task<Result<PagedResult<VentaDto>>> GetVentasAsync(
        string? estado, string? tipo, string? fechaDesde, string? fechaHasta,
        int? clienteId, int? personId, int page, int pageSize);

    Task<Result<VentaDto>> CrearVentaAsync(CrearVentaRequest request);
    Task<Result<VentaDto>> ActualizarVentaAsync(int id, ActualizarVentaRequest request);
    Task<Result<VentaDto>> ConfirmarVentaAsync(int id, int userId);
    Task<Result<VentaDto>> CancelarVentaAsync(int id, CancelarVentaRequest request);
    Task<Result<bool>> EliminarPresupuestoAsync(int id);
    Task<Result<VentaDto>> RegistrarCobroAsync(RegistrarCobroRequest request, int userId);
    Task<Result<VentaDto>> EmitirComprobanteAsync(int ventaId, int userId);
    Task<Result<VentaDto>> EmitirFacturaAsync(EmitirFacturaRequest request);
    Task<Result<List<VentaDto>>> GetCobrosPendientesAsync();

    // Devoluciones
    Task<Result<DevolucionDto>> SolicitarDevolucionAsync(int ventaId, SolicitarDevolucionRequest request, int userId, string userName);
    Task<Result<List<DevolucionDto>>> GetDevolucionesAsync(int ventaId);
    Task<Result<List<DevolucionDto>>> GetDevolucionesPendientesAsync();
    Task<Result<DevolucionDto>> GestionarDevolucionAsync(int devolucionId, GestionarDevolucionRequest request, int userId, string userName);
}
