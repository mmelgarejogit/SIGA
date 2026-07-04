namespace SIGA.Domain.Entities;

public class TransferenciaStock
{
    public int Id { get; set; }

    public int SucursalOrigenId { get; set; }
    public Sucursal SucursalOrigen { get; set; } = null!;

    public int SucursalDestinoId { get; set; }
    public Sucursal SucursalDestino { get; set; } = null!;

    public DateOnly Fecha { get; set; }
    public string Estado { get; set; } = "Pendiente"; // Pendiente | Aceptada | Rechazada

    public string? CreadoPorId { get; set; }
    public string? CreadoPorNombre { get; set; }
    public string? RecibidoPorNombre { get; set; }
    public string? Observaciones { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FechaResolucion { get; set; }

    public ICollection<TransferenciaStockItem> Items { get; set; } = new List<TransferenciaStockItem>();
}
