namespace SIGA.Application.DTOs.Inventario;

public class UpdateProductoRequest
{
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string? Sku { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public int StockMinimo { get; set; }
}
