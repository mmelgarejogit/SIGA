namespace SIGA.Domain.Entities;

public class Producto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public int? CategoriaProductoId { get; set; }
    public CategoriaProducto? CategoriaProducto { get; set; }

    public int? MarcaId { get; set; }
    public Marca? Marca { get; set; }

    public int? ModeloId { get; set; }
    public Modelo? Modelo { get; set; }

    public ICollection<ProductoVariante> Variantes { get; set; } = [];
    public ICollection<PedidoProveedorItem> PedidoItems { get; set; } = [];
}
