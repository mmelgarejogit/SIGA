namespace SIGA.Application.DTOs.Compras;

public class FacturaCompraItemResponse
{
    public int Id { get; set; }
    public int? ProductoId { get; set; }
    public string Descripcion { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Total { get; set; }
    public string TipoIva { get; set; } = "";
}
