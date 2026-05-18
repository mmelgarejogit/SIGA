namespace SIGA.Application.DTOs.Ventas;

public class EmitirFacturaRequest
{
    public int VentaId { get; set; }
    public string NumeroFactura { get; set; } = null!;
    public string Timbrado { get; set; } = null!;
    public string Establecimiento { get; set; } = null!;
    public string FechaEmision { get; set; } = null!;
    public string? Observaciones { get; set; }
}
