namespace SIGA.Application.DTOs.Inventario;

public class MovimientoStockResponse
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public int SucursalId { get; set; }
    public string? SucursalNombre { get; set; }
    public string Tipo { get; set; } = "";
    public int Cantidad { get; set; }
    public string? Motivo { get; set; }
    public int? MotivoMovimientoId { get; set; }
    public DateTime FechaMovimiento { get; set; }
    public string? CreadoPorNombre { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public string? AprobadoPorNombre { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string? ObservacionesAprobacion { get; set; }
    public DateTime CreatedAt { get; set; }
}
