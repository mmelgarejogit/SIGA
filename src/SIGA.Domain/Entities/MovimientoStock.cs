namespace SIGA.Domain.Entities;

public class MovimientoStock
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public string Tipo { get; set; } = ""; // Entrada | Salida | Ajuste
    public int Cantidad { get; set; }
    public string? Motivo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
