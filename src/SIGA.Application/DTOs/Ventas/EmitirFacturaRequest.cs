namespace SIGA.Application.DTOs.Ventas;

public class EmitirFacturaRequest
{
    public int VentaId { get; set; }
    public int TimbradoId { get; set; }
    public string FechaEmision { get; set; } = null!;
    public string? Observaciones { get; set; }
}