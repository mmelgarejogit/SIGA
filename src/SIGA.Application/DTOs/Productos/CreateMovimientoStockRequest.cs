namespace SIGA.Application.DTOs.Productos;

public class CreateMovimientoStockRequest
{
    public string Tipo { get; set; } = ""; // Entrada | Salida
    public int Cantidad { get; set; }
    public int? MotivoMovimientoId { get; set; }
    public DateTime? FechaMovimiento { get; set; }
}
