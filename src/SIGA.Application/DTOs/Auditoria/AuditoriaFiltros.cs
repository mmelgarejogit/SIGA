namespace SIGA.Application.DTOs.Auditoria;

public class AuditoriaFiltros
{
    public string? Categoria { get; set; }
    public string? Accion { get; set; }
    public int? UserId { get; set; }
    public string? FechaDesde { get; set; }   // "yyyy-MM-dd"
    public string? FechaHasta { get; set; }   // "yyyy-MM-dd"
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
