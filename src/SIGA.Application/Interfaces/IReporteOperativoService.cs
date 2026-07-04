using SIGA.Application.Common;
using SIGA.Application.DTOs.Reportes;

namespace SIGA.Application.Interfaces;

/// <summary>
/// Reportes operativos de control: listados fila-por-fila, filtrables y paginados, con totales.
/// El scoping de sucursal se resuelve por el usuario del request (ver implementación).
/// </summary>
public interface IReporteOperativoService
{
    Task<Result<ReporteOperativoDto<ReporteVentaRow>>> GetVentasAsync(ReporteOperativoFiltros f);
    Task<Result<ReporteOperativoDto<ReporteCompraRow>>> GetComprasAsync(ReporteOperativoFiltros f);
    Task<Result<ReporteOperativoDto<ReporteMovInventarioRow>>> GetMovInventarioAsync(ReporteOperativoFiltros f);
    Task<Result<ReporteOperativoDto<ReporteMovCajaRow>>> GetMovCajaAsync(ReporteOperativoFiltros f);

    /// <summary>Trae TODAS las filas del reporte (sin paginar) ya en forma tabular lista para exportar.</summary>
    Task<Result<ReporteExport>> GetExportAsync(string tipo, ReporteOperativoFiltros f);
}
