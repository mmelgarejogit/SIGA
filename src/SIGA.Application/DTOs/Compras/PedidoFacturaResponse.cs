namespace SIGA.Application.DTOs.Compras;

public class PedidoFacturaResponse
{
    public int Id { get; set; }
    public string? NroFactura { get; set; }
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
}
