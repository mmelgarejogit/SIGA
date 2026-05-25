namespace SIGA.Domain.Entities;

public class CategoriaProducto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = "";
    public string? Descripcion { get; set; }
    public decimal Margen { get; set; } = 0;
    public decimal Descuento { get; set; } = 0;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Producto> Productos { get; set; } = [];
}
