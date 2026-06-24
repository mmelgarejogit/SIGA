namespace SIGA.Domain.Entities;

public class MotivoMovimiento
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "Ambos"; // Entrada | Salida | Ambos
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<MovimientoStock> Movimientos { get; set; } = [];
}
