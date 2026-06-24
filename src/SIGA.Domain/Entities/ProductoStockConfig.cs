namespace SIGA.Domain.Entities;

public class ProductoStockConfig
{
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public int StockMinimo { get; set; }
    public int? StockMaximo { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
