namespace SIGA.Domain.Entities;

// Constancia de cada vez que un trabajo a pedido se manda a rehacer (garantía / defecto).
// No toca la venta del cliente: el re-trabajo es siempre sin costo para él. Preserva el historial
// (cuántas veces, por qué y quién asumió el costo) para responsabilidad y reportes.
public class RetrabajoTrabajoPedido
{
    public int Id { get; set; }

    public int TrabajoPedidoId { get; set; }
    public TrabajoPedido TrabajoPedido { get; set; } = null!;

    public DateOnly Fecha { get; set; }
    public MotivoRetrabajo Motivo { get; set; }
    public ResponsableRetrabajo Responsable { get; set; }
    public string? Observacion { get; set; }

    public int RegistradoPorId { get; set; }
    public User? RegistradoPor { get; set; }

    public DateTime CreatedAt { get; set; }
}
