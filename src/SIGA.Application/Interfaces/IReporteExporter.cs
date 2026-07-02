using SIGA.Application.DTOs.Reportes;

namespace SIGA.Application.Interfaces;

/// <summary>
/// Exporta una tabla genérica (<see cref="ReporteExport"/>) a PDF o CSV. Recibe el DTO (no el HTTP),
/// así lo puede reutilizar la descarga manual y un futuro envío automático por email.
/// </summary>
public interface IReporteExporter
{
    byte[] ToPdf(ReporteExport data);
    byte[] ToCsv(ReporteExport data);
}
