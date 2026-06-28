using SIGA.Application.Common;
using SIGA.Application.DTOs.Reportes;

namespace SIGA.Application.Interfaces;

public interface IReporteService
{
    Task<Result<ReporteVentasDto>> GetReporteVentasAsync(DateOnly desde, DateOnly hasta, string agrupacion);
    Task<Result<ReporteCitasDto>> GetReporteCitasAsync(DateOnly desde, DateOnly hasta, string agrupacion);
    Task<Result<ReporteInventarioDto>> GetReporteInventarioAsync(DateOnly desde, DateOnly hasta, string agrupacion);
    Task<Result<ReporteComprasDto>> GetReporteComprasAsync(DateOnly desde, DateOnly hasta, string agrupacion);
}
