namespace SIGA.Application.DTOs.Compras;

public class FacturaCompraResponse
{
    public int Id { get; set; }
    public string? NroFactura { get; set; }
    public int ProveedorId { get; set; }
    public string ProveedorNombre { get; set; } = "";
    public int? PedidoProveedorId { get; set; }
    public decimal MontoExento { get; set; }
    public decimal MontoGravado5 { get; set; }
    public decimal MontoGravado10 { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal Iva5 { get; set; }
    public decimal Iva10 { get; set; }
    public string CondicionVenta { get; set; } = "";
    public string Estado { get; set; } = "";
    public string FechaEmision { get; set; } = "";
    public string? FechaVencimiento { get; set; }
    public string? FechaPago { get; set; }
    public string? Observaciones { get; set; }
    public string? MotivoAnulacion { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool TieneRecepciones { get; set; }
}
