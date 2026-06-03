namespace SIGA.Application.DTOs.Productos;

public class ProductoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int? CategoriaProductoId { get; set; }
    public string? CategoriaNombre { get; set; }
    public decimal DescuentoCategoria { get; set; }
    public int? MarcaId { get; set; }
    public string? MarcaNombre { get; set; }
    public int? ModeloId { get; set; }
    public string? ModeloNombre { get; set; }
    public int TotalVariantes { get; set; }
}

public class ProductoVarianteResponse
{
    public Guid Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public string? Sku { get; set; }
    public string? Color { get; set; }
    public string? Talle { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public string? ImagenUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
