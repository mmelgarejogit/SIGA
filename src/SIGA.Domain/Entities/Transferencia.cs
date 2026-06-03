namespace SIGA.Domain.Entities;

public enum EstadoTransferencia { Solicitada, Aprobada, Rechazada }

public class Transferencia
{
    public Guid Id { get; set; }
    public Guid SucursalOrigenId { get; set; }
    public Sucursal SucursalOrigen { get; set; } = null!;
    public Guid SucursalDestinoId { get; set; }
    public Sucursal SucursalDestino { get; set; } = null!;
    public EstadoTransferencia Estado { get; set; } = EstadoTransferencia.Solicitada;
    public int SolicitadoPorId { get; set; }
    public User SolicitadoPor { get; set; } = null!;
    public int? AprobadoPorId { get; set; }
    public User? AprobadoPor { get; set; }
    public string? Observacion { get; set; }
    public string? MotivoRechazo { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime? FechaResolucion { get; set; }

    public ICollection<TransferenciaLinea> Lineas { get; set; } = [];
}
