namespace SIGA.Domain.Entities;

public class PedidoProveedor
{
    public int Id { get; set; }
    public int ProveedorId { get; set; }
    public Proveedor Proveedor { get; set; } = null!;
    public int? EstadoConfigId { get; set; }
    public EstadoConfig? EstadoConfig { get; set; }
    public string? Observaciones { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<PedidoProveedorItem> Items { get; set; } = [];
}
