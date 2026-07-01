namespace SIGA.Domain.Entities;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string? Sku { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public string? Color { get; set; }
    public string? Talle { get; set; }
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }

    public int? CategoriaProductoId { get; set; }
    public CategoriaProducto? CategoriaProducto { get; set; }

    public int? MarcaId { get; set; }
    public Marca? Marca { get; set; }

    public int? ModeloId { get; set; }
    public Modelo? Modelo { get; set; }

    public ProductoStockConfig? StockConfig { get; set; }

    public ICollection<MovimientoStock> Movimientos { get; set; } = [];
    public ICollection<PedidoProveedorItem> PedidoItems { get; set; } = [];

    /// <summary>
    /// Setea el costo y deriva el precio de venta aplicando el margen (%) de la categoría.
    /// El precio de venta es siempre calculado, nunca cargado a mano. Guaraníes sin decimales.
    /// </summary>
    public void AplicarCosto(decimal costo, decimal margen)
    {
        PrecioCosto = costo;
        PrecioVenta = Math.Round(costo * (1 + margen / 100m), 0, MidpointRounding.AwayFromZero);
    }
}
