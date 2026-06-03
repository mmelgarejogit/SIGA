namespace SIGA.Application.DTOs.Egresos;

public class CrearGastoGeneralRequest
{
    public Guid SucursalId { get; set; }
    public int CategoriaGastoId { get; set; }
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = "";
    public string? Observaciones { get; set; }
    public string FechaEmision { get; set; } = "";
    public string? FechaVencimiento { get; set; }
}