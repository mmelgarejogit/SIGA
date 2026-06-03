namespace SIGA.Application.DTOs.Inventario;

public class ProductoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string? Sku { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public int StockActual { get; set; }
    public int StockMinimo { get; set; }
    public int? StockMaximo { get; set; }
    public bool BajoStock { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public decimal DescuentoCategoria { get; set; }

    public int? MarcaId { get; set; }
    public string? MarcaNombre { get; set; }
    public int? ModeloId { get; set; }
    public string? ModeloNombre { get; set; }
    public string? Color { get; set; }
    public string? Talle { get; set; }
    public string? Descripcion { get; set; }
    public string? ImagenUrl { get; set; }
}
