namespace SIGA.Application.DTOs.Egresos;

public class CrearFacturaCompraRequest
{
    public int ProveedorId { get; set; }
    public int? PedidoProveedorId { get; set; }
    public string? NroFactura { get; set; }
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = "";
    public string? Observaciones { get; set; }
    public string FechaEmision { get; set; } = "";
    public string? FechaVencimiento { get; set; }
}
