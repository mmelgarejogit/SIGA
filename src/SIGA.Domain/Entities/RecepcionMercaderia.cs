namespace SIGA.Domain.Entities;

public class RecepcionMercaderia
{
    public int Id { get; set; }
    public int PedidoProveedorId { get; set; }
    public PedidoProveedor PedidoProveedor { get; set; } = null!;
    public string? Observaciones { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<RecepcionMercaderiaItem> Items { get; set; } = [];
}
