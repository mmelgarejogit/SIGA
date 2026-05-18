namespace SIGA.Application.DTOs.Ventas;

public class RegistrarCobroRequest
{
    public int VentaId { get; set; }
    public decimal Monto { get; set; }
    public string MetodoPago { get; set; } = "Efectivo";
    public string FechaCobro { get; set; } = null!;
    public string? Referencia { get; set; }
}
