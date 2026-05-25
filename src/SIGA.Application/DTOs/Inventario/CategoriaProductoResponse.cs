namespace SIGA.Application.DTOs.Inventario;

public class CategoriaProductoResponse
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public decimal Margen { get; set; }
    public decimal Descuento { get; set; }
    public bool IsActive { get; set; }
    public int TotalProductos { get; set; }
}

public class CreateCategoriaProductoRequest
{
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public decimal Margen { get; set; } = 0;
    public decimal Descuento { get; set; } = 0;
}

public class UpdateCategoriaProductoRequest
{
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public decimal Margen { get; set; } = 0;
    public decimal Descuento { get; set; } = 0;
    public bool IsActive { get; set; } = true;
}
