namespace SIGA.Domain.Entities;

public enum TipoDevolucion
{
    Devolucion = 1,
    Cambio     = 2,
}

public enum EstadoDevolucion
{
    Pendiente  = 1,
    Confirmada = 2,
    Rechazada  = 3,
}

public class Devolucion
{
    public int Id { get; set; }

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public TipoDevolucion Tipo { get; set; }
    public EstadoDevolucion Estado { get; set; } = EstadoDevolucion.Pendiente;
    public string Motivo { get; set; } = null!;

    public int SolicitadoPorId { get; set; }
    public User SolicitadoPor { get; set; } = null!;

    public int? ConfirmadoPorId { get; set; }
    public User? ConfirmadoPor { get; set; }

    public string? ObservacionesRevision { get; set; }
    public DateTime? FechaRevision { get; set; }

    public ICollection<DevolucionLinea> Lineas { get; set; } = new List<DevolucionLinea>();

    public DateTime CreatedAt { get; set; }
}
