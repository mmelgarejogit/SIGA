namespace SIGA.Domain.Entities;

public class DevolucionProveedor
{
    public int Id { get; set; }
    public int PedidoProveedorId { get; set; }
    public PedidoProveedor PedidoProveedor { get; set; } = null!;
    public int PedidoProveedorItemId { get; set; }
    public PedidoProveedorItem Item { get; set; } = null!;
    public int Cantidad { get; set; }
    public string Motivo { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
