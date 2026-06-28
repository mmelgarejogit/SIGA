namespace SIGA.Domain.Entities;

public class TransferenciaStockItem
{
    public int Id { get; set; }

    public int TransferenciaStockId { get; set; }
    public TransferenciaStock TransferenciaStock { get; set; } = null!;

    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;

    public int Cantidad { get; set; }
}
