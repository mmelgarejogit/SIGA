namespace SIGA.Domain.Entities;

public class ConteoInventario
{
    public int Id { get; set; }
    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }
    public int CreadoPorId { get; set; }
    public string CreadoPorNombre { get; set; } = "";
    public DateTime FechaConteo { get; set; } = DateTime.UtcNow;
    public string Estado { get; set; } = "Pendiente"; // Pendiente | Aprobado | Rechazado
    public string? Observaciones { get; set; }
    public string? AprobadoPorNombre { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string? ObservacionesAprobacion { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ConteoInventarioLinea> Lineas { get; set; } = [];
}
