namespace SIGA.Domain.Entities;

public class FacturaLaboratorio
{
    public int Id { get; set; }

    public int TrabajoPedidoId { get; set; }
    public TrabajoPedido TrabajoPedido { get; set; } = null!;

    public string NumeroFactura { get; set; } = null!;
    public string? Timbrado { get; set; }
    public DateOnly FechaEmision { get; set; }
    public decimal Monto { get; set; }
    public string? Observaciones { get; set; }

    public int EmitidoPorId { get; set; }
    public User EmitidoPor { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
}
