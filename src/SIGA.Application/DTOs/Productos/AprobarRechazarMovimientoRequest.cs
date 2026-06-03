namespace SIGA.Application.DTOs.Productos;

public class AprobarRechazarMovimientoRequest
{
    public string Estado { get; set; } = ""; // Aprobado | Rechazado
    public string? Observaciones { get; set; }
}
