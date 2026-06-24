namespace SIGA.Domain.Entities;

public class Modelo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public int MarcaId { get; set; }
    public Marca Marca { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Producto> Productos { get; set; } = [];
}
