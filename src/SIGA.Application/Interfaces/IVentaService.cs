using SIGA.Application.Common;
using SIGA.Application.DTOs.Ventas;

namespace SIGA.Application.Interfaces;

public interface IVentaService
{
    Task<Result<VentaDto>> CrearVentaAsync(CrearVentaRequest request);
    Task<Result<VentaDto>> ConfirmarVentaAsync(int id);
    Task<Result<VentaDto>> RegistrarCobroAsync(RegistrarCobroRequest request);
    Task<Result<VentaDto>> EmitirFacturaAsync(EmitirFacturaRequest request);
    Task<Result<VentaDto>> GetVentaByIdAsync(int id);
    Task<Result<PagedResult<VentaDto>>> GetVentasAsync(
        string? estado, string? fechaDesde, string? fechaHasta,
        int? patientId, int page, int pageSize);

    // Anulación con aprobación
    Task<Result<SolicitudAnulacionVentaDto>> SolicitarAnulacionAsync(
        int userId, string userName, SolicitarAnulacionRequest request);
    Task<Result<List<SolicitudAnulacionVentaDto>>> GetSolicitudesAnulacionAsync(string? estado);
    Task<Result<SolicitudAnulacionVentaDto>> GestionarAnulacionAsync(
        int solicitudId, int userId, string userName, GestionarAnulacionVentaRequest request);
}
