namespace SIGA.Application.DTOs.Productos;

public class CreateProductoRequest
{
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public int? CategoriaProductoId { get; set; }
    public int? MarcaId { get; set; }
    public int? ModeloId { get; set; }
}

public class CreateProductoVarianteRequest
{
    public int ProductoId { get; set; }
    public string? Sku { get; set; }
    public string? Color { get; set; }
    public string? Talle { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
}

public class UpdateProductoVarianteRequest
{
    public string? Sku { get; set; }
    public string? Color { get; set; }
    public string? Talle { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
    public bool IsActive { get; set; } = true;
}
