namespace SIGA.Application.DTOs.Inventario;

public class CreateMovimientoStockRequest
{
    public string Tipo { get; set; } = ""; // Entrada | Salida
    public int Cantidad { get; set; }
    public int? MotivoMovimientoId { get; set; }
    public DateTime? FechaMovimiento { get; set; }
}
