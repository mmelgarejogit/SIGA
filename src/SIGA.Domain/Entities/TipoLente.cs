namespace SIGA.Domain.Entities;

public class TipoLente
{
    public int Id { get; set; }
    public string Nombre { get; set; } = null!;

    /// <summary>Precio sugerido del lente para este diseño. Autocompleta y es editable por venta.</summary>
    public decimal PrecioBase { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<TrabajoPedido> TrabajosPedido { get; set; } = new List<TrabajoPedido>();
}
