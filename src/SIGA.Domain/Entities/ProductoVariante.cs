namespace SIGA.Domain.Entities;

public class ProductoVariante
{
    public Guid Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public string? Sku { get; set; }
    public string? Color { get; set; }
    public string? Talle { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public string? ImagenUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MovimientoInventario> Movimientos { get; set; } = [];
    public ICollection<ParametroStock> ParametrosStock { get; set; } = [];
    public ICollection<AjusteManual> AjustesManual { get; set; } = [];
    public ICollection<TransferenciaLinea> TransferenciaLineas { get; set; } = [];
    public ICollection<VentaLinea> VentaLineas { get; set; } = [];
    public ICollection<PedidoProveedorItem> PedidoItems { get; set; } = [];
    public ICollection<FacturaCompraItem> FacturaCompraItems { get; set; } = [];
}
