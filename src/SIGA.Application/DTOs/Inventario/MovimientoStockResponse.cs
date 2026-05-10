namespace SIGA.Application.DTOs.Inventario;

public class MovimientoStockResponse
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public string ProductoNombre { get; set; } = "";
    public string Tipo { get; set; } = "";
    public int Cantidad { get; set; }
    public string? Motivo { get; set; }
    public DateTime CreatedAt { get; set; }
}
