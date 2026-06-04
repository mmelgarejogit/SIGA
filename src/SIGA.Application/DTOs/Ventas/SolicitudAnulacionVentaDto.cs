namespace SIGA.Application.DTOs.Ventas;

public class SolicitudAnulacionVentaDto
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public string NumeroComprobante { get; set; } = "";
    public string PacienteNombre { get; set; } = "";
    public decimal TotalVenta { get; set; }
    public string EstadoVenta { get; set; } = "";
    public string Motivo { get; set; } = "";
    public string Estado { get; set; } = "";
    public string SolicitadoPorNombre { get; set; } = "";
    public string? RevisadoPorNombre { get; set; }
    public string? ObservacionesRevision { get; set; }
    public DateTime? FechaRevision { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SolicitarAnulacionRequest
{
    public int VentaId { get; set; }
    public string Motivo { get; set; } = "";
}

public class GestionarAnulacionVentaRequest
{
    public string Accion { get; set; } = ""; // Aprobada | Rechazada
    public string? ObservacionesRevision { get; set; }
}
