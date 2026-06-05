namespace SIGA.Application.DTOs.Ventas;

public class CrearVentaRequest
{
    public int PatientId { get; set; }
    public int? RecetaId { get; set; }
    public string Tipo { get; set; } = "Directa";
    public string CondicionVenta { get; set; } = "Contado";
    public string FechaVenta { get; set; } = null!;
    public string? Observaciones { get; set; }
    public List<AgregarLineaRequest> Lineas { get; set; } = new();
}
