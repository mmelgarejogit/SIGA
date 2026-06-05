namespace SIGA.Domain.Entities;

public class TipoLente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<TrabajoPedido> TrabajosPedido { get; set; } = new List<TrabajoPedido>();
}
