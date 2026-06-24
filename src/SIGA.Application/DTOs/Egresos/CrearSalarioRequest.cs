namespace SIGA.Application.DTOs.Egresos;

public class CrearSalarioRequest
{
    public int EmpleadoId { get; set; }
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = "";
    public string? Periodo { get; set; }
    public string? Observaciones { get; set; }
    public string FechaEmision { get; set; } = "";
    public string? FechaVencimiento { get; set; }
}
