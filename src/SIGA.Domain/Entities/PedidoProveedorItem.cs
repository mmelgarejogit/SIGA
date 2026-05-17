namespace SIGA.Domain.Entities;

public class PedidoProveedorItem
{
    public int Id { get; set; }
    public int PedidoProveedorId { get; set; }
    public PedidoProveedor PedidoProveedor { get; set; } = null!;
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int Cantidad { get; set; }
    public int CantidadRecibida { get; set; }
    public decimal PrecioUnitario { get; set; }

    public ICollection<DevolucionProveedor> Devoluciones { get; set; } = [];
}
