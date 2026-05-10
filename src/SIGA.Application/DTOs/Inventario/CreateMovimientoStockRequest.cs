namespace SIGA.Application.DTOs.Inventario;

public class CreateMovimientoStockRequest
{
    public string Tipo { get; set; } = ""; // Entrada | Salida | Ajuste
    public int Cantidad { get; set; }
    public string? Motivo { get; set; }
}
