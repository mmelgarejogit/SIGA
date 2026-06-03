namespace SIGA.Application.DTOs.Ventas;

public class AgregarLineaRequest
{
    public string Tipo { get; set; } = "Producto";
    public Guid? ProductoVarianteId { get; set; }
    public int? ServicioId { get; set; }
    public string? Descripcion { get; set; }
    public int Cantidad { get; set; } = 1;
    public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; } = 0;
    public string CategoriaFiscal { get; set; } = "Gravado10";
}
