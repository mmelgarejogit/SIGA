namespace SIGA.Application.DTOs.Egresos;

public class CrearHonorarioRequest
{
    public Guid SucursalId { get; set; }
    public int ProfessionalId { get; set; }
    public decimal Monto { get; set; }
    public string Concepto { get; set; } = "";
    public int PeriodoMes { get; set; }
    public int PeriodoAnio { get; set; }
    public string? Observaciones { get; set; }
    public string FechaEmision { get; set; } = "";
    public string? FechaVencimiento { get; set; }
}