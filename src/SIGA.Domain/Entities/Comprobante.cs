namespace SIGA.Domain.Entities;

public enum TipoComprobante
{
    ReciboSimple = 1,
}

public enum EstadoComprobante
{
    Emitido = 1,
    Anulado = 2,
}

public class Comprobante
{
    public int Id { get; set; }

    public int VentaId { get; set; }
    public Venta Venta { get; set; } = null!;

    public TipoComprobante Tipo { get; set; } = TipoComprobante.ReciboSimple;
    public EstadoComprobante Estado { get; set; } = EstadoComprobante.Emitido;

    public string? MotivoAnulacion { get; set; }

    public int EmitidoPorId { get; set; }
    public User EmitidoPor { get; set; } = null!;

    public int? AnuladoPorId { get; set; }
    public User? AnuladoPor { get; set; }

    public DateTime FechaEmision { get; set; }
    public DateTime? FechaAnulacion { get; set; }

    public DateTime CreatedAt { get; set; }
}
