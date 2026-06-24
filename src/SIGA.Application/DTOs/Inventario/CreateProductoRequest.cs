namespace SIGA.Application.DTOs.Inventario;

public class CreateProductoRequest
{
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string? Sku { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public int StockMinimo { get; set; }

    public int? MarcaId { get; set; }
    public int? ModeloId { get; set; }
    public string? Color { get; set; }
    public string? Talle { get; set; }
    public string? Descripcion { get; set; }
}
