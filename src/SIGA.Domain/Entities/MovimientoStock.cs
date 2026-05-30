namespace SIGA.Domain.Entities;

public class MovimientoStock
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public string Tipo { get; set; } = ""; // Entrada | Salida
    public int Cantidad { get; set; }
    public string? Motivo { get; set; }
    public int? MotivoMovimientoId { get; set; }
    public MotivoMovimiento? MotivoMovimiento { get; set; }
    public DateTime FechaMovimiento { get; set; } = DateTime.UtcNow;
    public string? CreadoPorId { get; set; }
    public string? CreadoPorNombre { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente | Aprobado | Rechazado
    public string? AprobadoPorNombre { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string? ObservacionesAprobacion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
