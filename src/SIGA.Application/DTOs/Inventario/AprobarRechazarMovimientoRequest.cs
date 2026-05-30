namespace SIGA.Application.DTOs.Inventario;

public class AprobarRechazarMovimientoRequest
{
    public string Estado { get; set; } = ""; // Aprobado | Rechazado
    public string? Observaciones { get; set; }
}
