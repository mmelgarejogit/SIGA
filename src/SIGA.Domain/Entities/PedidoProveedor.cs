namespace SIGA.Domain.Entities;

public class PedidoProveedor
{
    public int Id { get; set; }
    public int SucursalId { get; set; }
    public Sucursal? Sucursal { get; set; }
    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public EstadoPedido Estado { get; set; } = EstadoPedido.Borrador;
    public string? Observaciones { get; set; }
    public DateOnly? FechaOrden { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PedidoProveedorItem> Items { get; set; } = [];
    public ICollection<DevolucionProveedor> Devoluciones { get; set; } = [];
    public ICollection<RecepcionMercaderia> Recepciones { get; set; } = [];
}
