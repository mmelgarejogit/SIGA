namespace SIGA.Domain.Entities;

public class SolicitudAnulacionVenta
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public string Motivo { get; set; } = "";
    public string Estado { get; set; } = "Pendiente"; // Pendiente | Aprobada | Rechazada

    public int SolicitadoPorId { get; set; }
    public string SolicitadoPorNombre { get; set; } = "";

    /// Estado de la venta antes de poner AnulacionPendiente — para restaurar si se rechaza
    public string EstadoPrevioVenta { get; set; } = "";

    public string? RevisadoPorNombre { get; set; }
    public string? ObservacionesRevision { get; set; }
    public DateTime? FechaRevision { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
